using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using Gatherer.Models;
using Gatherer.Views;

namespace Gatherer.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        public ObservableCollection<SearchCriteria> Items { get; set; }
        public Command LoadItemsCommand { get; set; }

        public SearchViewModel()
        {
            Title = "Search";
            Items = new ObservableCollection<SearchCriteria>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        public void AddCriteria()
        {
            this.Items.Add(new SearchCriteria());
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}