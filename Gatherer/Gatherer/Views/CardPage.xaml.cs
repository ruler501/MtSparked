using Gatherer.Models;
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
        }
	}
}