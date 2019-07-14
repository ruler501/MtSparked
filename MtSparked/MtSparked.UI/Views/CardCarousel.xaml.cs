using MtSparked.Interop.Models;
using MtSparked.Core.Services;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.UI.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CardCarousel : ContentPage
	{

        private IList<Card> Cards { get; }

		public CardCarousel (IList<Card> cards, int index) {
            this.InitializeComponent();

            this.Carousel.ItemsSource = cards;
            this.Carousel.ScrollTo(index);
		}
        /* TODO: Figure out how to implement with Xamarin Forms 4.1
        private async void AllWithArt(object sender, EventArgs e)
            => await this.Navigation.PushAsync(new CardsListPage(CardDataStore.Where("IllustrationId", "Equals",
                this.Carousel.
                                                                                     this.Cards[this.Carousel.Position].IllustrationId).ToDataStore()));
        */
    }
}