using Gatherer.Models;
using Gatherer.Services;
using Gatherer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeakEvent;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Gatherer.ViewModels.DeckViewModel;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeckPage : ContentPage
	{
        Deck Deck;
        DeckViewModel viewModel;

        public DeckPage()
            : this(ConfigurationManager.ActiveDeck)
        { }

        public DeckPage (Deck deck = null)
		{
            InitializeComponent ();

            Deck = ConfigurationManager.ActiveDeck = deck;

            this.BindingContext = viewModel = new DeckViewModel(Deck);
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem is null)
            {
                return;
            }
           // Manually deselect item.
           ((ListView)sender).SelectedItem = null;
            if(args.SelectedItem is Card card)
            {
                await Navigation.PushAsync(new CardPage(card, null, -1));
            }
            else if(args.SelectedItem is CardWithBoard cwb)
            {
                await Navigation.PushAsync(new CardPage(Deck.Boards[cwb.Board][cwb.Id].Card, null, -1));
            }
        }
    }
}