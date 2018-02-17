using Gatherer.Models;
using Gatherer.Services;
using Gatherer.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Gatherer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchGroupView : ContentView
    {
        SearchViewModel viewModel;

        public ObservableCollection<string> Items { get; set; }

        public SearchGroupView()
        {
            InitializeComponent();

            BindingContext = viewModel = new SearchViewModel();
        }

        async void AddItem(object sender, EventArgs e)
        {
            this.viewModel.AddCriteria();
        }

        async void AddGroup(object sender, EventArgs e)
        {
            return;
        }

        public CardDataStore.CardsQuery CreateQuery()
        {
            CardDataStore.CardsQuery query = new CardDataStore.CardsQuery(viewModel.Connector);
            foreach (SearchCriteria criteria in viewModel.Items)
            {
                query.Where(criteria.Field, criteria.Operation, criteria.Value);
            }
            return query;
        }
    }
}
