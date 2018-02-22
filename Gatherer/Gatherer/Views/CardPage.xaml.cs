using Gatherer.Models;
using Gatherer.Services;
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
        Card Card;

        Dictionary<string, Label> FoilCounters;
        Dictionary<string, Label> NormalCounters;
        List<Card> cards;
        int index;
        
        public CardPage (Card card, List<Card> cards, int index)
		{
			InitializeComponent ();

            this.BindingContext = Card = card;

            this.FoilCounters = new Dictionary<string, Label>();
            this.NormalCounters = new Dictionary<string, Label>();

            this.ToolbarItems.Clear();
            ToolbarItem allWithArt = new ToolbarItem()
            {
                Text = "All With Same Art"
            };
            allWithArt.Clicked += this.AllWithArt;
            this.ToolbarItems.Add(allWithArt);
            if(!(cards is null))
            {
                this.cards = cards;
                this.index = index;
                if(index > 0)
                {
                    ToolbarItem prev = new ToolbarItem()
                    {
                        Text = "Prev"
                    };
                    prev.Clicked += this.PrevCard;
                    this.ToolbarItems.Add(prev);
                }
                if(index >=0 && index < cards.Count - 1)
                {
                    ToolbarItem next = new ToolbarItem()
                    {
                        Text = "Next"
                    };
                    next.Clicked += this.NextCard;

                    this.ToolbarItems.Add(next);
                }
            }

            int row = 3;
            Deck deck = ConfigurationManager.ActiveDeck;
            foreach(string board in deck.BoardNames.OrderBy(n => n != Deck.MASTER))
            {
                string capturableBoard = board;

                this.GridView.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });
                Button minus = new Button()
                {
                    Text = "-",
                    FontSize = 10,
                    Margin = 0
                };
                minus.Clicked += (_s, _e) => this.RemoveCard(capturableBoard, false);
                this.GridView.Children.Add(minus, 0, row);
                Label foil = new Label()
                {
                    FontSize = 16,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                this.GridView.Children.Add(foil, 1, row);
                this.FoilCounters[board] = foil;
                Button plus = new Button()
                {
                    Text = "+",
                    FontSize = 10,
                    Margin = 0
                };
                plus.Clicked += (_s, _e) => this.AddCard(capturableBoard, false);
                this.GridView.Children.Add(plus, 2, row);

                this.GridView.Children.Add(new Label()
                {
                    Text = board,
                    FontSize = 14,
                    HorizontalTextAlignment = TextAlignment.Center
                }, 3, row);

                minus = new Button()
                {
                    Text = "-",
                    FontSize = 10,
                    Margin = 0
                };
                minus.Clicked += (_s, _e) => this.RemoveCard(capturableBoard, true);
                this.GridView.Children.Add(minus, 4, row);
                Label normal = new Label()
                {
                    FontSize = 16,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                this.GridView.Children.Add(normal, 5, row);
                this.NormalCounters[board] = normal;
                plus = new Button()
                {
                    Text = "+",
                    FontSize = 10,
                    Margin = 0
                };
                plus.Clicked += (_s, _e) => this.AddCard(capturableBoard, true);
                this.GridView.Children.Add(plus, 6, row);
                row += 1;
            }

            this.UpdateCounts();

            ConfigurationManager.ActiveDeck.ChangeEvent += this.UpdateCounts;
        }

        public void UpdateCounts(object sender=null, DeckChangedEventArgs args=null)
        {
            foreach (KeyValuePair<string, Label> pair in NormalCounters)
            {
                string board = pair.Key;
                Label counter = pair.Value;
                if (ConfigurationManager.ShowUnique)
                {
                    counter.Text = ConfigurationManager.ActiveDeck.GetNormalCountByName(Card, board).ToString();
                }
                else
                {
                    counter.Text = ConfigurationManager.ActiveDeck.GetNormalCount(Card, board).ToString();
                }
            }
            foreach (KeyValuePair<string, Label> pair in FoilCounters)
            {
                string board = pair.Key;
                Label counter = pair.Value;
                if (ConfigurationManager.ShowUnique)
                {
                    counter.Text = ConfigurationManager.ActiveDeck.GetFoilCountByName(Card, board).ToString();
                }
                else
                {
                    counter.Text = ConfigurationManager.ActiveDeck.GetFoilCount(Card, board).ToString();
                }
            }
        }

        void RemoveCard(string board, bool normal)
        {
            ConfigurationManager.ActiveDeck.RemoveCard(this.Card, board, normal);
        }

        void AddCard(string board, bool normal)
        {
            ConfigurationManager.ActiveDeck.AddCard(this.Card, board, normal);
        }

        async void PrevCard(object sender, EventArgs e)
        {
            if (index <= 0) return;
            if (Navigation.NavigationStack.Count == 0 ||
                !(Navigation.NavigationStack.Last() is CardPage cp && cp.index == index - 1))
            {
                await Navigation.PushAsync(new CardPage(cards[index - 1], cards, index - 1));
                Navigation.RemovePage(this);
            }
        }

        async void NextCard(object sender, EventArgs e)
        {
            if (index < 0 || index >= cards.Count - 1) return;
            if (Navigation.NavigationStack.Count == 0 ||
                !(Navigation.NavigationStack.Last() is CardPage cp && cp.index == index + 1))
            {
                await Navigation.PushAsync(new CardPage(cards[index + 1], cards, index + 1));
                Navigation.RemovePage(this);
            }
        }

        async void AllWithArt(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CardsListPage(CardDataStore.Where("IllustrationId", "Equals", this.Card.IllustrationId).ToDataStore()));
        }

        void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Running)
            {
                if (e.TotalY < 40 && e.TotalY > -40)
                {
                    // this.ScrollView.TranslationX = translatedX + e.TotalX;
                    if (e.TotalX > 80)
                    {
                        this.PrevCard(null, null);
                    }
                    if (e.TotalX < -80)
                    {
                        this.NextCard(null, null);
                    }
                }
            }
        }
    }
}