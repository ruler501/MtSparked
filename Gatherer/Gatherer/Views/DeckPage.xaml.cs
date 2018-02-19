using Gatherer.Models;
using Gatherer.Services;
using Gatherer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeakEvent;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Gatherer.ViewModels.DeckViewModel;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeckPage : ContentPage
	{
        Deck Deck;
        DeckViewModel viewModel;

        public DeckPage()
            : this(null)
        { }

        public DeckPage (Deck deck = null)
		{
            InitializeComponent ();

            if (deck is null) {
                deck = new Deck
                {
                    Name = "Lilis"
                };

                List<Card> cards = CardDataStore.Where("Name", "Contains", "Liliana").ToList();
                foreach (Card card in cards)
                {
                    deck.AddCard(card, "Mainboard");
                }
                cards = CardDataStore.Where("Name", "Contains", "Nixilis").ToList();
                foreach (Card card in cards)
                {
                    deck.AddCard(card, "Sideboard", false, 2);
                }
            }

            Deck = ConfigurationManager.ActiveDeck = deck;

            this.BindingContext = viewModel = new DeckViewModel(Deck);
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem is null)
            {
                return;
            }
           // Manually deselect item.
           ((ListView)sender).SelectedItem = null;
            if(args.SelectedItem is Card card)
            {
                await Navigation.PushAsync(new CardPage(card));
            }
            else if(args.SelectedItem is CardWithBoard cwb)
            {
                await Navigation.PushAsync(new CardPage(Deck.Boards[cwb.Board][cwb.Id].Card));
            }
        }
    }
}