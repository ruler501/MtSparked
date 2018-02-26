using MtSparked.Models;
using MtSparked.Services;
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
	public partial class CardView : ContentView
	{
        Card Card;
        
        public CardView()
		{
			InitializeComponent ();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            this.Card = this.BindingContext as Card;

            if (this.Card is null) return;
            
            this.UpdateCounts();

            ConfigurationManager.ActiveDeck.ChangeEvent += this.UpdateCounts;
        }

        public void UpdateCounts(object sender=null, DeckChangedEventArgs args=null)
        {
            int row = 3;
            foreach (View child in this.GridView.Children.Where(c => Grid.GetRow(c) >= row).ToList())
            {
                this.GridView.Children.Remove(child);
            }
            while (this.GridView.RowDefinitions.Count > row)
            {
                this.GridView.RowDefinitions.RemoveAt(row);
            }

            Deck deck = ConfigurationManager.ActiveDeck;
            foreach (Deck.BoardInfo boardInfo in deck.BoardInfos)
            {
                if (!boardInfo.Editable) continue;

                string name = boardInfo.Name;

                this.GridView.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });
                Button minus = new Button()
                {
                    Text = "-",
                    FontSize = 10,
                    Margin = 0
                };
                minus.Clicked += (_s, _e) => this.RemoveCard(name, false);
                this.GridView.Children.Add(minus, 0, row);
                Label foil = new Label()
                {
                    FontSize = 16,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                if (ConfigurationManager.ShowUnique)
                {
                    foil.Text = ConfigurationManager.ActiveDeck.GetFoilCountByName(this.Card, name).ToString();
                }
                else
                {
                    foil.Text = ConfigurationManager.ActiveDeck.GetFoilCount(this.Card, name).ToString();
                }
                this.GridView.Children.Add(foil, 1, row);
                Button plus = new Button()
                {
                    Text = "+",
                    FontSize = 10,
                    Margin = 0
                };
                plus.Clicked += (_s, _e) => this.AddCard(name, false);
                this.GridView.Children.Add(plus, 2, row);

                this.GridView.Children.Add(new Label()
                {
                    Text = name,
                    FontSize = 14,
                    HorizontalTextAlignment = TextAlignment.Center
                }, 3, row);

                minus = new Button()
                {
                    Text = "-",
                    FontSize = 10,
                    Margin = 0
                };
                minus.Clicked += (_s, _e) => this.RemoveCard(name, true);
                this.GridView.Children.Add(minus, 4, row);
                Label normal = new Label()
                {
                    FontSize = 16,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                this.GridView.Children.Add(normal, 5, row);
                if (ConfigurationManager.ShowUnique)
                {
                    normal.Text = ConfigurationManager.ActiveDeck.GetNormalCountByName(this.Card, name).ToString();
                }
                else
                {
                    normal.Text = ConfigurationManager.ActiveDeck.GetNormalCount(this.Card, name).ToString();
                }
                plus = new Button()
                {
                    Text = "+",
                    FontSize = 10,
                    Margin = 0
                };
                plus.Clicked += (_s, _e) => this.AddCard(name, true);
                this.GridView.Children.Add(plus, 6, row);
                row += 1;
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
    }
}