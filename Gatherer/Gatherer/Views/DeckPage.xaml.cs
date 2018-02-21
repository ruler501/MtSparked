using Acr.UserDialogs;
using Gatherer.FilePicker;
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
            const string NAME_DECK = "Name Deck";
            const string OPEN_DECK = "Open Deck";
            const string SAVE_DECK_AS = "Save Deck As";
            const string SHARE_DECK = "Share Deck";
            const string IMPORT_FROM_DEC = "Import from .dec(Unsupported)";
            const string EXPORT_TO_DEC = "Export to .dec(Unsupported)";
            const string SHARE_AS_DEC = "Share as .dec(Unsupported)";
            const string MANAGE_BOARDS = "Manage Visible Boards(Unsupported)";
            const string ADD_BOARD = "Add Board";
            const string REMOVE_BOARD_PREFIX = "Remove Board: ";
            List<string> actions = new List<string>()
            {
                NEW_DECK, NAME_DECK, OPEN_DECK, SAVE_DECK_AS, SHARE_DECK, IMPORT_FROM_DEC, EXPORT_TO_DEC, SHARE_AS_DEC, MANAGE_BOARDS, ADD_BOARD
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
            else if(action == NAME_DECK)
            {
                PromptResult name = await this.Dialogs.PromptAsync(new PromptConfig().SetMessage("Deck Name"));
                if (name.Ok)
                {
                    this.Deck.Name = name.Text;
                }
            }
            else if(action == OPEN_DECK)
            {
                FileData fileData = await ConfigurationManager.FilePicker.OpenFileAs();
                if(fileData is null)
                {
                    await DisplayAlert("Error", "Failed to open Deck File", "OK");
                }
                else
                {
                    if (fileData.FilePath != this.Deck.StoragePath)
                    {
                        string toRelease = this.Deck.StoragePath;
                        ConfigurationManager.ActiveDeck = this.Deck = new Deck(fileData.FilePath);
                        this.BindingContext = viewModel = new DeckViewModel(this.Deck);
                        ConfigurationManager.FilePicker.ReleaseFile(toRelease);
                    }
                }
            }
            else if(action == SAVE_DECK_AS)
            {
                this.Deck.SaveDeckAs();
            }
            else if(action == SHARE_DECK)
            {
                ConfigurationManager.FilePicker.ShareFile(this.Deck.StoragePath);
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

        public void ToggleUnique(object sender, EventArgs args)
        {
            ConfigurationManager.ShowUnique = !ConfigurationManager.ShowUnique;
        }
    }
}