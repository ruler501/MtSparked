using Gatherer.Models;
using Gatherer.Services;
using Gatherer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CardsListPage : ContentPage
	{
        CardsViewModel viewModel;

        public CardsListPage()
        {
            InitializeComponent();

            var query = CardDataStore.Where("TypeLine", "Contains", "Planeswalker").Find();

            BindingContext = viewModel = new CardsViewModel(query);
        }

        public CardsListPage(CardDataStore cards)
        {
            InitializeComponent();
            
            BindingContext = viewModel = new CardsViewModel(cards);
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if(args.SelectedItem is null)
            {
                return;
            }
            Card card = (Card)args.SelectedItem;
            Navigation.PushAsync(new CardPage(card));
            // Manually deselect item.
            ((ListView)sender).SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}