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
	public partial class CardCarousel : ContentPage
	{
        IList<Card> Cards { get; set; }

		public CardCarousel (IList<Card> cards, int index)
		{
			InitializeComponent ();

            this.Carousel.ItemsSource = cards;
            this.Carousel.Position = index;
		}
        
        async void AllWithArt(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CardsListPage(CardDataStore.Where("IllustrationId", "Equals", this.Cards[this.Carousel.Position].IllustrationId).ToDataStore()));
        }
    }
}