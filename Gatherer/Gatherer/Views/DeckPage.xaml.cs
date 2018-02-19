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
                deck.AddCard(card, "Mainboard");
            }
            cards = CardDataStore.Where("Name", "Contains", "Nixilis").ToList();
            foreach(Card card in cards)
            {
                deck.AddCard(card, "Sideboard", false, 2);
            }

            this.BindingContext = Deck = ConfigurationManager.ActiveDeck = deck;

            this.CreateChildren();

            Deck.ChangeEvent += this.CreateChildren;
        }

        public void CreateChildren(object sender=null, DeckChangedEventArgs args=null)
        {
            this.StackLayout.Children.Clear();

            foreach(KeyValuePair<string, IDictionary<string, Deck.BoardItem>> pair in this.Deck.Boards.OrderBy(p => p.Key))
            {
                string name = pair.Key;
                if (name == Deck.MASTER) continue;
                IEnumerable<Deck.BoardItem> cards = pair.Value.Values.OrderBy(bi => bi.Card.Cmc).ThenBy(bi => bi.Card.Name);
                StackLayout boardLayout = new StackLayout();
                boardLayout.Children.Add(new Label
                {
                    Text = name,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 14
                });

                foreach (Deck.BoardItem card in cards)
                {
                    boardLayout.Children.Add(new CardItemView(card.Card));
                }

                this.StackLayout.Children.Add(boardLayout);
            }

            foreach(Deck.BoardItem card in this.Deck.Master.Values)
            {
                this.StackLayout.Children.Add(new CardItemView(card.Card));
            }
        }
	}
}