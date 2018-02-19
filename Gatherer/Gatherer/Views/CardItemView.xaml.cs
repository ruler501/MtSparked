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
	public partial class CardItemView : ContentView
	{
        Card Card;

		public CardItemView (Card card)
		{
			InitializeComponent ();

            this.BindingContext = Card = card;

            this.UpdateCount();

            ConfigurationManager.ActiveDeck.ChangeEvent += this.UpdateCount;
		}

        public void UpdateCount(object sender=null, DeckChangedEventArgs args = null)
        {
            this.Count.Text = ConfigurationManager.ActiveDeck.GetCount(this.Card).ToString();
        }
    }
}