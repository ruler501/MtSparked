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
        Dictionary<string, Label> FoilLabels = new Dictionary<string, Label>();
        Dictionary<string, Label> NormalLabels = new Dictionary<string, Label>();

        public CardView()
		{
			InitializeComponent ();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            this.Card = this.BindingContext as Card;

            if (this.Card is null) return;

            this.SetLabel.Text = this.Card.SetName + " / " + this.Card.SetCode;

            this.ColorIdentityLabel.Text = "ColorIdentity: " + (this.Card.ColorIdentity.Length > 0 ? this.Card.ColorIdentity : "Colorless");
            this.EdhRankLabel.Text = this.Card.EdhRank is null ? "" : "EDHRank: " + this.Card.EdhRank.ToString();
            if (!String.IsNullOrWhiteSpace(this.Card.Power))
            {
                this.PowerToughnessLabel.Text = this.Card.Power + "/" + this.Card.Toughness;
            }
            else if (!(this.Card.Life is null))
            {
                this.PowerToughnessLabel.Text = (this.Card.Hand < 0 ? "-" : "+") + this.Card.Hand + "/" + (this.Card.Life < 0 ? "-" : "+") + this.Card.Life;
            }
            else if (!(this.Card.Loyalty is null))
            {
                this.PowerToughnessLabel.Text = this.Card.Loyalty.ToString();
            }

            if (!(this.Card.Watermark is null))
            {
                this.WatermarkLabel.Text = "Watermark: " + this.Card.Watermark;
            }

            this.ArtistLabel.Text = "Illustrated by " + this.Card.Artist;
            this.MarketLabel.Text = this.Card.MarketPrice is null ? "No Pricing" : ("Market: $" + this.Card.MarketPrice.ToString());

            this.BorderLabel.Text = this.Card.Border ?? "No" + " Bordered";

            this.FrameLabel.Text = "Frame: " + this.Card.Frame ?? "None";
            this.LayoutLabel.Text = "Layout: " + this.Card.Layout ?? "None";

            Color legal = new Color(117 / 255.0, 152 / 255.0, 110 / 255.0);
            Color notLegal = new Color(204 / 255.0, 125 / 255.0, 131 / 255.0);

            this.StandardLabel.BackgroundColor = this.Card.LegalInStandard ? legal : notLegal;
            this.StandardLabel.Text = "Standard: " + (this.Card.LegalInStandard ? "Y" : "N");

            this.ModernLabel.BackgroundColor = this.Card.LegalInModern ? legal : notLegal;
            this.ModernLabel.Text = "Modern: " + (this.Card.LegalInModern ? "Y" : "N");

            this.LegacyLabel.BackgroundColor = this.Card.LegalInLegacy ? legal : notLegal;
            this.LegacyLabel.Text = "Legacy: " + (this.Card.LegalInLegacy ? "Y" : "N");

            this.CommanderLabel.BackgroundColor = this.Card.LegalInCommander ? legal : notLegal;
            this.CommanderLabel.Text = "Commander: " + (this.Card.LegalInCommander ? "Y" : "N");

            this.DuelCommanderLabel.BackgroundColor = this.Card.LegalInDuelCommander ? legal : notLegal;
            this.DuelCommanderLabel.Text = "Duel Commander: " + (this.Card.LegalInDuelCommander ? "Y" : "N");

            this.MtgoCommanderLabel.BackgroundColor = this.Card.LegalInMtgoCommander ? legal : notLegal;
            this.MtgoCommanderLabel.Text = "Mtgo Commander: " + (this.Card.LegalInMtgoCommander ? "Y" : "N");

            this.PauperLabel.BackgroundColor = this.Card.LegalInPauper ? legal : notLegal;
            this.PauperLabel.Text = "Pauper: " + (this.Card.LegalInPauper ? "Y" : "N");

            this.PennyDreadfulLabel.BackgroundColor = this.Card.LegalInPennyDreadful ? legal : notLegal;
            this.PennyDreadfulLabel.Text = "Penny Dreadful: " + (this.Card.LegalInPennyDreadful ? "Y" : "N");

            this.FrontierLabel.BackgroundColor = this.Card.LegalInFrontier ? legal : notLegal;
            this.FrontierLabel.Text = "Frontier: " + (this.Card.LegalInFrontier ? "Y" : "N");

            this.NextStandardLabel.BackgroundColor = this.Card.LegalInNextStandard ? legal : notLegal;
            this.NextStandardLabel.Text = "Next Standard: " + (this.Card.LegalInNextStandard ? "Y" : "N");

            this.UpdateCounters();

            ConfigurationManager.ActiveDeck.ChangeEvent += this.OnDeckUpdated;
        }

        public void OnDeckUpdated(object sender, DeckChangedEventArgs args)
        {
            if(args is BoardChangedEventArgs)
            {
                this.UpdateCounters();
            }
            else if(args is CardCountChangedEventArgs)
            {
                this.UpdateCounts();
            }
        }

        public void UpdateCounters()
        {
            this.FoilLabels.Clear();
            this.NormalLabels.Clear();
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
                this.FoilLabels.Add(name, foil);
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
                this.NormalLabels.Add(name, normal);
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

        public void UpdateCounts()
        {
            foreach(KeyValuePair<string, Label> pair in this.FoilLabels)
            {
                string name = pair.Key;
                Label foil = pair.Value;

                if (ConfigurationManager.ShowUnique)
                {
                    foil.Text = ConfigurationManager.ActiveDeck.GetFoilCountByName(this.Card, name).ToString();
                }
                else
                {
                    foil.Text = ConfigurationManager.ActiveDeck.GetFoilCount(this.Card, name).ToString();
                }
            }

            foreach (KeyValuePair<string, Label> pair in this.NormalLabels)
            {
                string name = pair.Key;
                Label normal = pair.Value;

                if (ConfigurationManager.ShowUnique)
                {
                    normal.Text = ConfigurationManager.ActiveDeck.GetNormalCountByName(this.Card, name).ToString();
                }
                else
                {
                    normal.Text = ConfigurationManager.ActiveDeck.GetNormalCount(this.Card, name).ToString();
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
    }
}