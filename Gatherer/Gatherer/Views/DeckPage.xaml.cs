using Acr.UserDialogs;
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
        IUserDialogs Dialogs { get; set; }

        public DeckPage()
            : this(ConfigurationManager.ActiveDeck)
        { }

        public DeckPage (Deck deck = null)
		{
            InitializeComponent ();

            Deck = ConfigurationManager.ActiveDeck = deck;

            this.BindingContext = viewModel = new DeckViewModel(Deck);

            this.Dialogs = UserDialogs.Instance;
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

        async void ManageDeck(object sender, EventArgs args)
        {
            const string NEW_DECK = "New Deck";
            const string NAME_DECK = "Name Deck(Unsupported)";
            const string SAVE_DECK_AS = "Save Deck As";
            const string OPEN_DECK = "Open Deck(Unsupported)";
            const string EXPORT_TO_DEC = "Export to .dec(Unsupported)";
            const string SHARE_DECK = "Share Deck(Unsupported)";
            const string MANAGE_BOARDS = "Manage Visible Boards(Unsupported)";
            const string ADD_BOARD = "Add Board";
            const string REMOVE_BOARD_PREFIX = "Remove Board: ";
            List<string> actions = new List<string>()
            {
                NEW_DECK, NAME_DECK, SAVE_DECK_AS, OPEN_DECK, EXPORT_TO_DEC, SHARE_DECK, MANAGE_BOARDS, ADD_BOARD
            };
            foreach(string name in Deck.BoardNames.Where(n => n != Deck.MASTER))
            {
                actions.Add(REMOVE_BOARD_PREFIX + name);
            }
            string action = await DisplayActionSheet("Manage Deck", "Cancel", null, actions.ToArray());

            if(action == NEW_DECK)
            {
                this.Deck = ConfigurationManager.ActiveDeck = new Deck();
                this.BindingContext = this.viewModel = new DeckViewModel(this.Deck);
            }
            else if(action == SAVE_DECK_AS)
            {
                this.Deck.SaveDeckAs();
            }
            else if(action == ADD_BOARD)
            {
                PromptResult result = await this.Dialogs.PromptAsync(new PromptConfig().SetMessage("Board Name"));
                if (result.Ok)
                {
                    this.Deck.AddBoard(result.Text);
                }
            }
            else if (action.StartsWith(REMOVE_BOARD_PREFIX))
            {
                string name = action.Substring(REMOVE_BOARD_PREFIX.Length);
                this.Deck.RemoveBoard(name);
            }
        }
    }
}