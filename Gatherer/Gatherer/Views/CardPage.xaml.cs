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
	public partial class CardPage : ContentPage
	{
        CardsViewModel viewModel;

        public CardPage()
        {
            InitializeComponent();

            var query = CardDataStore.Where("TypeLine", "Contains", "Planeswalker").Find();

            BindingContext = viewModel = new CardsViewModel(query);
        }

        public CardPage(CardDataStore cards)
        {
            InitializeComponent();
            
            BindingContext = viewModel = new CardsViewModel(cards);
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            // Manually deselect item.
            CardsListView.SelectedItem = null;
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}