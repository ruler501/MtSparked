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
    public class SearchCriteriaDataStore : IDataStore<SearchCriteria>
    {
        List<SearchCriteria> items;

        public SearchCriteriaDataStore()
        {
            items = new List<SearchCriteria>();
        }

        public async Task<bool> AddItemAsync(SearchCriteria item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public Task<bool> UpdateItemAsync(SearchCriteria item)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteItemAsync(SearchCriteria item)
        {
            throw new NotImplementedException();
        }

        public Task<SearchCriteria> GetItemAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SearchCriteria>> GetItemsAsync(bool forceRefresh = false)
        {
            return Task.FromResult<IEnumerable<SearchCriteria>>(items);
        }
    }
}