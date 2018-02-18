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

		public CardPage (Card card)
		{
			InitializeComponent ();

            this.BindingContext = Card = card;

            this.UpdateCounts();

            ConfigurationManager.ActiveDeck.ChangeEvent += this.UpdateCounts;
        }

        public void UpdateCounts(object sender=null, DeckChangedEventArgs args=null)
        {
            this.NormalLabel.Text = ConfigurationManager.ActiveDeck.GetNormalCount(Card).ToString();
            this.FoilLabel.Text = ConfigurationManager.ActiveDeck.GetFoilCount(Card).ToString();
        }

        async void AddNormal(object sender, EventArgs e)
        {
            ConfigurationManager.ActiveDeck.AddCard(Card, true, 1);
        }

        async void RemoveNormal(object sender, EventArgs e)
        {
            ConfigurationManager.ActiveDeck.RemoveCard(Card, true, 1);
        }

        async void AddFoil(object sender, EventArgs e)
        {
            ConfigurationManager.ActiveDeck.AddCard(Card, false, 1);
        }

        async void RemoveFoil(object sender, EventArgs e)
        {
            ConfigurationManager.ActiveDeck.RemoveCard(Card, false, 1);
        }
    }
}