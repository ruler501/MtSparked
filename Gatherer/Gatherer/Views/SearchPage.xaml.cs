using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Gatherer.Models;
using Gatherer.Views;
using Gatherer.ViewModels;
using Gatherer.Services;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchPage : ContentPage
	{
        SearchViewModel viewModel;

        public SearchPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new SearchViewModel();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as SearchCriteria;
            if (item == null)
                return;

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            this.viewModel.AddCriteria();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }

        async void Search(object sender, EventArgs e)
        {
            CardDataStore.CardsQuery query = new CardDataStore.CardsQuery();
            foreach(SearchCriteria criteria in viewModel.Items)
            {
                query.Where(criteria.Field, criteria.Operation, criteria.Value);
            }
            await Navigation.PushAsync(new CardPage(query.Find()));
            return;
        }
    }
}