using MtSparked.Interop.Models;
using MtSparked.Core.Services;
using MtSparked.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MtSparked.UI.Views.Search;

namespace MtSparked.UI.Views {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CardsListPage : ContentPage {
        private CardsViewModel ViewModel { get; }
        private CardDataStore CardStore { get; }

        public CardsListPage(CardDataStore cards) {
            this.InitializeComponent();
            this.CardStore = cards;

            this.BindingContext = this.ViewModel = new CardsViewModel(this.CardStore);
        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs args) {
            if(args.SelectedItem is null) {
                return;
            }
            // TODO: Is there a way to avoid doing this?
            // Manually deselect item.
            ((ListView)sender).SelectedItem = null;
            Card card = (Card)args.SelectedItem;
            IEnumerable<Card> cards = new List<Card>();
            foreach(EnhancedGrouping<Card> grouping in ViewModel.Items) {
                cards = cards.Concat(grouping);
            }
            List<Card> cardsList = cards.ToList();
            await Navigation.PushAsync(new CardCarousel(cardsList, cardsList.IndexOf(card)));
        }

        protected override void OnAppearing() {
            base.OnAppearing();

            if (this.ViewModel.Items.Count == 0) {
                this.ViewModel.LoadItemsCommand.Execute(null);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
        public void ToggleUnique(object sender, EventArgs args) {
            ConfigurationManager.ShowUnique = !ConfigurationManager.ShowUnique;
        }

        public async void SortBy(object sender, EventArgs args) => await Navigation.PushAsync(new SortPage());

    }
}