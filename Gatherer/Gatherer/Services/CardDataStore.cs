using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Gatherer.Models;
using Realms;

[assembly: Xamarin.Forms.Dependency(typeof(Gatherer.Services.CardDataStore))]
namespace Gatherer.Services
{
    public class CardDataStore : IDataStore<Card>
    {
        List<Card> items;
        Expression<Func<Card, bool>> Query;

        private static Realm realm = Realm.GetInstance("cards.db");
        public static string DatabasePath => realm.Config.DatabasePath;

        protected CardDataStore(Expression<Func<Card, bool>> query)
        {
            this.Query = query;
            items = new List<Card>();
            var mockItems = realm.All<Card>().Where(Query).OrderBy(c => c.Cmc).ThenBy(c => c.Name);

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

        public async Task<IEnumerable<Card>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(realm.All<Card>().Where(Query).OrderBy(c => c.Cmc).ThenBy(c => c.Name));
        }
        
        public class CardsQuery
        {
            Expression builtExpression = null;
            ParameterExpression param = Expression.Parameter(typeof(Card));

            public CardsQuery Where(string field, string op, object value)
            {
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
                else if(op == "Less Than")
                {
                    fullCombine = Expression.LessThan(property, constant);
                }
                else if(op == "Greater Than")
                {
                    fullCombine = Expression.GreaterThan(property, constant);
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (this.builtExpression is null)
                {
                    this.builtExpression = fullCombine;
                }
                else
                {
                    this.builtExpression = Expression.AndAlso(builtExpression, fullCombine);
                }

                return this;
            }

            public CardDataStore Find()
            {
                return new CardDataStore(Expression.Lambda<Func<Card, bool>>(this.builtExpression, param));
            }
        }

        public static CardsQuery Where(string field, string op, object value)
        {
            return new CardsQuery().Where(field, op, value);
        }
    }
}