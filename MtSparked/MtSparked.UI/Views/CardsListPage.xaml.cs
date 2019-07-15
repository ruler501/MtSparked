using MtSparked.Interop.Models;
using MtSparked.Interop.Services;
using MtSparked.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MtSparked.UI.Views.Search;
using MtSparked.Interop.Databases;

namespace MtSparked.UI.Views {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CardsListPage : ContentPage {
        private CardsViewModel ViewModel { get; }
        private DataStore<Card> CardStore { get; }

        public CardsListPage(DataStore<Card> cards) {
            this.InitializeComponent();
            this.CardStore = cards;

            this.BindingContext = this.ViewModel = new CardsViewModel(this.CardStore);
        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs args) {
            if (args.SelectedItem is null) {
                return;
            }
            // TODO #82: Investigate Not Manually Setting SelectedItem to null in OnItemSelected
            // Manually deselect item.
            ((ListView)sender).SelectedItem = null;
            Card card = (Card)args.SelectedItem;
            IEnumerable<Card> cards = new List<Card>();
            foreach (EnhancedGrouping<Card> grouping in this.ViewModel.Items) {
                cards = cards.Concat(grouping);
            }
            List<Card> cardsList = cards.ToList();
            await this.Navigation.PushAsync(new CardCarousel(cardsList, cardsList.IndexOf(card)));
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

        public async void SortBy(object sender, EventArgs args) => await this.Navigation.PushAsync(new SortPage());

    }
}