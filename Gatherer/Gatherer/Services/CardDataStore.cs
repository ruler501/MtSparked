using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Gatherer.Models;
using Realms;

namespace Gatherer.Services
{
    public class CardDataStore
    {
        List<Card> items;
        Expression<Func<Card, bool>> Query;

        private static Realm realm = null;

        static CardDataStore()
        {
            RealmConfiguration config = new RealmConfiguration("cards.db");
            var t = config.DatabasePath;
            realm = Realm.GetInstance(config);

            var test = realm.All<Card>();
        }

        protected CardDataStore(Expression<Func<Card, bool>> query)
        {
            this.Query = query;
            items = new List<Card>();
            var mockItems = realm.All<Card>().Where(Query).OrderBy(c => c.Cmc).ThenBy(c => c.Name);

            foreach (var item in mockItems)
            {
                items.Add(item);
                if(items.Count > 250)
                {
                    break;
                }
            }
        }

        public async Task<bool> AddItemAsync(Card item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Card item)
        {
            var _item = items.Where((Card arg) => arg.MultiverseId == item.MultiverseId).FirstOrDefault();
            items.Remove(_item);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(Card item)
        {
            var _item = items.Where((Card arg) => arg.MultiverseId == item.MultiverseId).FirstOrDefault();
            items.Remove(_item);

            return await Task.FromResult(true);
        }

        public async Task<Card> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.MultiverseId == id));
        }

        public async Task<IEnumerable<Card>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(realm.All<Card>().Where(Query).OrderBy(c => c.Cmc).ThenBy(c => c.Name));
        }
        
        public class CardsQuery
        {
            Expression builtExpression = null;
            static ParameterExpression param = Expression.Parameter(typeof(Card), "card");
            string Connector;

            public CardsQuery Where(string field, string op, string value)
            {
                if(value is null || string.IsNullOrWhiteSpace(value))
                {
                    return this;
                }
                Expression property = Expression.Property(param, field);
                Expression constant = Expression.Constant(value);
                Expression fullCombine = null;
                if(op == "Equals")
                {
                    fullCombine = Expression.Equal(property, constant);
                }
                else if(op == "Contains")
                {
                    Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                    fullCombine = Expression.Call(typeof(StringExtensions), "Contains", Type.EmptyTypes, property, constant, caseInsensitive);
                }
                else if(op == "Like")
                {
                    fullCombine = Expression.Call(typeof(StringExtensions), "Like", Type.EmptyTypes, property, constant);
                }
                else if(op == "Starts With")
                {
                    Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                    fullCombine = Expression.Call(property, "StartsWith", Type.EmptyTypes, constant, caseInsensitive);
                }
                else if (op == "Ends With")
                {
                    Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                    fullCombine = Expression.Call(property, "EndsWith", Type.EmptyTypes, constant, caseInsensitive);
                }
                else if(op == "Less Than")
                {
                    fullCombine = Expression.LessThan(property, constant);
                }
                else if(op == "Greater Than")
                {
                    fullCombine = Expression.GreaterThan(property, constant);
                }
                else if(op == "Exists")
                {
                    Expression nullConstant = Expression.Constant(null);
                    fullCombine = Expression.NotEqual(property, nullConstant);
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (this.builtExpression is null)
                {
                    this.builtExpression = fullCombine;
                }
                else if(Connector == "All")
                {
                    this.builtExpression = Expression.AndAlso(builtExpression, fullCombine);
                }
                else if (Connector == "Any")
                {
                    this.builtExpression = Expression.OrElse(builtExpression, fullCombine);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return this;
            }

            public CardsQuery Where(CardsQuery other)
            {
                if(other is null || other.builtExpression is null)
                {
                    throw new ArgumentNullException();
                }
                Expression otherExpression = other.builtExpression;
                if(this.builtExpression is null)
                {
                    this.builtExpression = other.builtExpression;
                }
                else if (Connector == "All")
                {
                    this.builtExpression = Expression.AndAlso(builtExpression, otherExpression);
                }
                else if (Connector == "Any")
                {
                    this.builtExpression = Expression.OrElse(builtExpression, otherExpression);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return this;
            }

            public CardsQuery Negate()
            {
                if(this.builtExpression is null)
                {
                    throw new Exception("Cannot negate an empty query");
                }
                else
                {
                    this.builtExpression = Expression.Not(this.builtExpression);
                }

                return this;
            }

            private Expression<Func<Card, bool>> BuildExpression()
            {
                if(this.builtExpression is null)
                {
                    return Expression.Lambda<Func<Card, bool>>(Expression.Constant(true), param);
                }

                return Expression.Lambda<Func<Card, bool>>(this.builtExpression, param);
            }

            public CardDataStore ToDataStore()
            {
                return new CardDataStore(this.BuildExpression());
            }

            public List<Card> ToList()
            {
                return realm.All<Card>().Where(this.BuildExpression()).ToList();
            }

            public CardsQuery(string connector)
            {
                this.Connector = connector;
            }
        }

        public static CardsQuery Where(string field, string op, string value, string connector="All")
        {
            return new CardsQuery(connector).Where(field, op, value);
        }

        public static Card ById(string id)
        {
            return realm.Find<Card>(id);
        }
    }
}