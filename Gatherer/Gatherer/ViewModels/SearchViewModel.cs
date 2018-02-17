using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using Gatherer.Models;
using Gatherer.Views;

namespace Gatherer.ViewModels
{
    public class SearchViewModel : ContentPage
    {
        public ObservableCollection<SearchCriteria> Items { get; set; }
        public string Connector { get; set; }

        public SearchViewModel()
        {
            Title = "Search";
            Items = new ObservableCollection<SearchCriteria>();
            this.Connector = "All";
            this.Items.Add(new SearchCriteria());
        }

        public void AddCriteria()
        {
            this.Items.Add(new SearchCriteria());
        }
    }
}