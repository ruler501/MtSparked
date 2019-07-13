using MtSparked.Models;
using MtSparked.Services;
using MtSparked.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CardsListPage : ContentPage
	{
        CardsViewModel viewModel;
        CardDataStore Cards;

        public CardsListPage(CardDataStore cards)
        {
            InitializeComponent();
            this.Cards = cards;
            
            BindingContext = viewModel = new CardsViewModel(this.Cards);
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if(args.SelectedItem is null)
            {
                return;
            }
            // Manually deselect item.
            ((ListView)sender).SelectedItem = null;
            Card card = (Card)args.SelectedItem;
            IEnumerable<Card> cards = new List<Card>();
            foreach(EnhancedGrouping<Card> grouping in viewModel.Items)
            {
                cards = cards.Concat(grouping);
            }
            List<Card> cardsList = cards.ToList();
            await Navigation.PushAsync(new CardCarousel(cardsList, cardsList.IndexOf(card)));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }

        public void ToggleUnique(object sender, EventArgs args)
        {
            ConfigurationManager.ShowUnique = !ConfigurationManager.ShowUnique;
        }

        public async void SortBy(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SortPage());
        }
    }
}