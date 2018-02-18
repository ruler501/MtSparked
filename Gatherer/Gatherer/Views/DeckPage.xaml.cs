using Gatherer.Models;
using Gatherer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeakEvent;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeckPage : ContentPage
	{
        Deck Deck;

        public DeckPage ()
		{
            InitializeComponent ();

            Deck deck = new Deck
            {
                Name = "Lilis"
            };

            List<Card> cards = CardDataStore.Where("Name", "Contains", "Liliana").ToList();
            foreach(Card card in cards)
            {
                deck.AddCard(card);
            }

            this.BindingContext = Deck = ConfigurationManager.ActiveDeck = deck;

            this.CreateChildren();
		}

        public void CreateChildren()
        {
            this.StackLayout.Children.Clear();

            foreach(Deck.DeckCard card in this.Deck.Cards.Values)
            {
                this.StackLayout.Children.Add(new CardItemView(card.Card));
            }
        }
	}
}