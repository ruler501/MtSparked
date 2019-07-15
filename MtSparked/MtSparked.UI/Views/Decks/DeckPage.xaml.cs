using Acr.UserDialogs;
using MtSparked.Interop.Services;
using MtSparked.Interop.Models;
using MtSparked.UI.ViewModels;
using MtSparked.Core.Decks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MtSparked.Interop.FileSystem;

namespace MtSparked.UI.Views.Decks {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeckPage : ContentPage {

        private Deck Deck { get; set; }
        private DeckViewModel ViewModel { get; set; }
        private bool Active { get; } = false;
        private IUserDialogs Dialogs { get; set; }

        public DeckPage()
            : this(ConfigurationManager.ActiveDeck, true)
        {}

        public DeckPage (Deck deck, bool active = true) {
            this.InitializeComponent ();
            this.Dialogs = UserDialogs.Instance;

            this.Active = active;
            if (active) {
                this.Deck = ConfigurationManager.ActiveDeck = deck;
            } else {
                this.Deck = deck;
                this.ManageToolbarItem.Text = "Save As";
            }

            this.BindingContext = this.ViewModel = new DeckViewModel(this.Deck);
        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs args) {
            if (args.SelectedItem is null) {
                return;
            }
            // TODO #82: Investigate Not Manually Setting SelectedItem to null in OnItemSelected
            // Manually deselect item.
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem is DeckViewModel.CardWithBoard cwb) {
                DeckViewModel.Board board = this.ViewModel.BoardByName[cwb.Board];
                int index = board.IndexOf(cwb);
                List<Card> cards = board.Select(icwb => this.Deck.Boards[icwb.Board][icwb.Id].Card).ToList();

                await this.Navigation.PushAsync(new CardCarousel(cards, index));
            } else {
                System.Diagnostics.Debug.WriteLine("Strangeness is happeneing");
            }
        }

        private async void ManageDeck(object sender, EventArgs args) {
            if (!this.Active) {
                this.Deck.SaveDeckAs();
                return;
            }
            const string NEW_DECK = "New Deck";
            const string NAME_DECK = "Name Deck";
            const string OPEN_DECK = "Open Deck";
            const string SAVE_DECK_AS = "Save Deck As";
            // const string SHARE_DECK = "Share Deck";
            const string IMPORT_FROM_DEC = "Import from .dec";
            const string EXPORT_TO_DEC = "Export to .dec";
            // const string SHARE_AS_DEC = "Share as .dec(Unsupported)";
            const string MANAGE_BOARDS = "Manage Visible Boards";
            const string ADD_BOARD = "Add Board";
            const string REMOVE_BOARD_PREFIX = "Remove Board: ";
            List<string> actions = new List<string>() {
                NAME_DECK, OPEN_DECK, SAVE_DECK_AS, IMPORT_FROM_DEC, EXPORT_TO_DEC, MANAGE_BOARDS, ADD_BOARD
            };
            foreach(string name in this.Deck.BoardNames.Where(n => n != Deck.MASTER)) {
                actions.Add(REMOVE_BOARD_PREFIX + name);
            }
            string action = await this.Dialogs.ActionSheetAsync("Manage Deck", "Cancel", NEW_DECK, null, actions.ToArray());

            // TODO #89: More Extensible Way to Manage Deck Menu Actions
            if (action == NEW_DECK) {
                this.Deck = ConfigurationManager.ActiveDeck = new Deck();
                this.BindingContext = this.ViewModel = new DeckViewModel(this.Deck);
            } else if(action == NAME_DECK) {
                PromptResult name = await this.Dialogs.PromptAsync(new PromptConfig().SetMessage("Deck Name").SetCancellable(true));
                if (name.Ok) {
                    this.Deck.Name = name.Text;
                }
            } else if(action == OPEN_DECK) {
                FileData fileData = await ConfigurationManager.FilePicker.OpenFileAs();
                if(fileData is null) {
                    await this.DisplayAlert("Error", "Failed to open Deck File", "Okay");
                } else {
                    if (fileData.FilePath != this.Deck.StoragePath) {
                        string toRelease = this.Deck.StoragePath;
                        ConfigurationManager.ActiveDeck = this.Deck = DeckFormats.FromJdec(fileData.FilePath);
                        this.BindingContext = this.ViewModel = new DeckViewModel(this.Deck);
                        ConfigurationManager.FilePicker.ReleaseFile(toRelease);
                    }
                }
            } else if(action == SAVE_DECK_AS) {
                this.Deck.SaveDeckAs();
            } else if(action == IMPORT_FROM_DEC) {
                FileData fileData = await ConfigurationManager.FilePicker.OpenFileAs();
                if (fileData is null) {
                    await this.DisplayAlert("Error", "Failed to import .dec File", "Okay");
                } else {
                    if (fileData.FilePath != this.Deck.StoragePath) {
                        string toRelease = this.Deck.StoragePath;
                        ConfigurationManager.ActiveDeck = this.Deck = DeckFormats.FromDec(fileData.FilePath);
                        this.BindingContext = this.ViewModel = new DeckViewModel(this.Deck);
                        ConfigurationManager.FilePicker.ReleaseFile(toRelease);
                    }
                }
            } else if(action == EXPORT_TO_DEC) {
                this.Deck.SaveAsDec();
            } else if (action == MANAGE_BOARDS) {
                await this.Navigation.PushAsync(new BoardEditing(this.Deck));
            } else if(action == ADD_BOARD) {
                PromptResult result = await this.Dialogs.PromptAsync(new PromptConfig().SetMessage("Board Name"));
                if (result.Ok) {
                    this.Deck.AddBoard(result.Text);
                }
            } else if (action.StartsWith(REMOVE_BOARD_PREFIX)) {
                string name = action.Substring(REMOVE_BOARD_PREFIX.Length);
                this.Deck.RemoveBoard(name);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
        public void ToggleUnique(object sender, EventArgs args) {
            ConfigurationManager.ShowUnique = !ConfigurationManager.ShowUnique;
        }

        public async void OpenStats(object sender, EventArgs args) => await this.Navigation.PushAsync(new StatsPage(this.Deck));
    }
}