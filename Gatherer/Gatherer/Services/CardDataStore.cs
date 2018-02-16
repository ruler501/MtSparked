using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gatherer.Models;
using Realms;

[assembly: Xamarin.Forms.Dependency(typeof(Gatherer.Services.CardDataStore))]
namespace Gatherer.Services
{
    public class CardDataStore : IDataStore<Card>
    {
        List<Card> items;

        private static Realm realm = Realm.GetInstance("cards.db");

        public CardDataStore()
        {
            items = new List<Card>();
            var mockItems = realm.All<Card>().Where(c => c.Name.Contains("Jace"));

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
            return await Task.FromResult(realm.All<Card>().Where(c => c.Name.Contains("Jace")));
        }
    }
}