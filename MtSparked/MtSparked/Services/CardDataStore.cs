using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using MtSparked.Models;
using Realms;

namespace MtSparked.Services
{
    public class CardDataStore
    {
        public List<Card> items;
        Expression<Func<Card, bool>> Query;

        private static Realm realm = null;

        static CardDataStore()
        {
            RealmConfiguration config = new RealmConfiguration("cards.db");
            realm = Realm.GetInstance(config);
        }

        protected CardDataStore(Expression<Func<Card, bool>> query)
        {
            this.Query = query;

            this.LoadCards();
        }

        public void LoadCards()
        {
            items = new List<Card>(250);

            IEnumerable<Card> mockItems = realm.All<Card>().Where(Query).OrderBy(c => c.Cmc).ThenBy(c => c.Name);

            if (ConfigurationManager.ShowUnique)
            {
                mockItems = mockItems.DistinctBy(c => c.Name);
            }

            foreach (var item in mockItems)
            {
                items.Add(item);
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
        
        public class CardsQuery
        {
            Expression builtExpression = null;
            static ParameterExpression param = Expression.Parameter(typeof(Card), "card");
            string Connector;

            public CardsQuery Where(string field, bool set)
            {
                field = field.Replace(" ", "");
                Expression property = Expression.Property(param, field);
                Expression constant = Expression.Constant(set, typeof(bool));

                Expression fullCombine = Expression.Equal(property, constant);

                if (this.builtExpression is null)
                {
                    this.builtExpression = fullCombine;
                }
                else if (Connector == "All")
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

            public CardsQuery Where(string field, string op, string value)
            {
                if(value is null || string.IsNullOrWhiteSpace(value))
                {
                    return this;
                }
                field = field.Replace(" ", "");
                value = value.Trim();
                if (field.Contains("Color"))
                {
                    if (value.Equals("White", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "W";
                    }
                    else if (value.Equals("Blue", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "U";
                    }
                    else if (value.Equals("Black", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "B";
                    }
                    else if (value.Equals("Red", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "R";
                    }
                    else if (value.Equals("Green", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "G";
                    }
                }

                Expression property = Expression.Property(param, field);
                Expression constant = Expression.Constant(value);
                PropertyInfo propertyInfo = typeof(Card).GetProperty(field);
                if(propertyInfo.PropertyType == typeof(bool))
                {
                    bool val = bool.Parse(value);
                    constant = Expression.Constant(val, propertyInfo.PropertyType);
                }
                else if(propertyInfo.PropertyType == typeof(int?))
                {
                    constant = Expression.Constant(Int32.Parse(value), typeof(int?));
                }
                else if (propertyInfo.PropertyType == typeof(int))
                {
                    constant = Expression.Constant(Int32.Parse(value), typeof(int));
                }
                else if(propertyInfo.PropertyType == typeof(double?))
                {
                    constant = Expression.Constant(Double.Parse(value), typeof(double?));
                }
                else if (propertyInfo.PropertyType == typeof(double))
                {
                    constant = Expression.Constant(Double.Parse(value), typeof(double));
                }
                Expression fullCombine = null;
                if(op == "Equals")
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                        fullCombine = Expression.Call(property, "Equals", Type.EmptyTypes, new[] { constant, caseInsensitive });
                    }
                    else
                    {
                        fullCombine = Expression.Equal(property, constant);
                    }
                }
                else if(op == "Contains")
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                        fullCombine = Expression.Call(typeof(StringExtensions), "Contains", Type.EmptyTypes, property, constant, caseInsensitive);
                    }
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
                    bool exists = Boolean.Parse(value);
                    if (exists)
                    {
                        fullCombine = Expression.NotEqual(property, nullConstant);
                    }
                    else
                    {
                        fullCombine = Expression.Equal(property, nullConstant);
                    }
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
        public static CardsQuery Where(string field, bool set, string connector = "All")
        {
            return new CardsQuery(connector).Where(field, set);
        }

        public static Card ById(string id)
        {
            return realm.Find<Card>(id);
        }

        public static Card ByMvid(string mvid)
        {
            return realm.All<Card>().Where(c => c.MultiverseId == mvid).FirstOrDefault();
        }
    }
}