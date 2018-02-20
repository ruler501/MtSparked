using Gatherer.Models;
using Gatherer.Services;
using Gatherer.ViewModels;
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
	public partial class CardItemView : ViewCell
	{
		public CardItemView ()
		{
			InitializeComponent ();

            this.UpdateCount();

            ConfigurationManager.ActiveDeck.ChangeEvent += this.UpdateCount;
		}

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            this.UpdateCount();
        }

        public void UpdateCount(object sender=null, DeckChangedEventArgs args = null)
        {
            if (this.BindingContext is null) return;
            string board = Deck.MASTER;
            string id = null;
            if (this.BindingContext is DeckViewModel.CardWithBoard cwb)
            {
                board = cwb.Board;
                id = cwb.Id;
            }
            else if(this.BindingContext is Card c)
            {
                id = c.Id;
            }
            this.Count.Text = ConfigurationManager.ActiveDeck.GetCount(id, board).ToString();
        }
    }
}