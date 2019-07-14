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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private IList<Card> Cards { get; }

        public CardCarousel(IList<Card> cards, int index) {
            this.InitializeComponent();

            this.Carousel.ItemsSource = cards;
            this.Carousel.ScrollTo(index);
		}
        /* TODO #80: Investigate CarouselView Providers
        private async void AllWithArt(object sender, EventArgs e)
            => await this.Navigation.PushAsync(new CardsListPage(CardDataStore.Where("IllustrationId", "Equals",
                this.Carousel.
                                                                                     this.Cards[this.Carousel.Position].IllustrationId).ToDataStore()));
        */
    }
}