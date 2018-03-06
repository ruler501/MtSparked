using Acr.UserDialogs;
using MtSparked.FilePicker;
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
	public partial class CubePage : ContentPage
    {
        IUserDialogs Dialogs { get; set; }
        string Path { get; set; }

        public CubePage ()
		{
			InitializeComponent ();

            this.Dialogs = UserDialogs.Instance;

            this.Path = ConfigurationManager.ActiveCubePath;
            if (ConfigurationManager.FilePicker.PathExists(this.Path))
            {

                byte[] contents = ConfigurationManager.FilePicker.OpenFile(this.Path);
                this.CubeDefEditor.Text = Encoding.UTF8.GetString(contents);
            }
        }

        public void OnPackNumChanged(object sender, TextChangedEventArgs args)
        {
            if(!String.IsNullOrWhiteSpace(args.NewTextValue) && args.NewTextValue != "Number of Packs")
            {
                bool valid = Int32.TryParse(args.NewTextValue, out int packNum);

                if (!valid)
                {
                    valid = Int32.TryParse(args.OldTextValue, out packNum);
                    if (valid)
                    {
                        this.PackNumEntry.Text = args.OldTextValue;
                    }
                    else
                    {
                        this.PackNumEntry.Text = "";
                    }
                }
            }
        }

        public async void GeneratePacks(object sender, EventArgs args)
        {
            bool valid = Int32.TryParse(this.PackNumEntry.Text, out int packNum);
            if (!valid || packNum < 0)
            {
                await this.DisplayAlert("Invalid Pack Number", $"'{this.PackNumEntry.Text}' is not a valid number", "Okay");
                return;
            }

            CubeParser parser;
            try
            {
                parser = new CubeParser(this.CubeDefEditor.Text, ConfigurationManager.ActiveDeck);
            }
            catch(Exception exc)
            {
                await this.DisplayAlert("Failed to Compile", $"Error: ${exc.Message}", "Okay");
                return;
            }

            List<List<PackCard>> cards = new List<List<PackCard>>(); 
            try
            {
                for(int i=0; i < packNum; i++)
                {
                    List<PackCard> pack = parser.MakePack();
                    cards.Add(pack);
                }
            }
            catch(Exception exc)
            {
                await this.DisplayAlert("Failed to Exectue", $"Error: ${exc.Message}", "Okay");
                return;
            }

            Deck generated = new Deck(false)
            {
                Name = "Generated Packs"
            };

            for(int i=0; i < cards.Count; i++)
            {
                foreach(PackCard card in cards[i])
                {
                    generated.AddCard(card.Card, Deck.MASTER, !card.Foil);
                    generated.AddCard(card.Card, $"Pack {i+1}", !card.Foil);
                }
            }
            generated.RemoveBoard("Mainboard");
            generated.RemoveBoard("Sideboard");

            foreach(Deck.BoardInfo info in generated.BoardInfos)
            {
                info.Editable = info.Viewable = info.Name != Deck.MASTER;
            }

            await Navigation.PushAsync(new DeckPage(generated, false));
        }

        public async void ManageCubeDef(object sender, EventArgs args)
        {
            const string NEW_CUBE_DEF = "New Cube Definition";
            const string OPEN_CUBE_DEF = "Open Cube Definition";
            const string SAVE_CUBE_DEF_AS = "Save Cube Definition As";
            const string ACCESS_HELP = "See Reference/Help";
            string[] actions = new[] { OPEN_CUBE_DEF, SAVE_CUBE_DEF_AS, ACCESS_HELP };
            string action = await this.Dialogs.ActionSheetAsync("Manage Cube Definition", "Cancel", NEW_CUBE_DEF, null, actions);

            if(action == NEW_CUBE_DEF)
            {
                this.Path = ConfigurationManager.ActiveCubePath = ConfigurationManager.DefaultCubeDefPath;
                this.CubeDefEditor.Text = "";
            }
            else if(action == OPEN_CUBE_DEF)
            {
                FileData data = await ConfigurationManager.FilePicker.OpenFileAs();
                if(data is null)
                {
                    await DisplayAlert("Error", "Failed to open Cube Definition", "Okay");
                }
                else
                {
                    this.Path = ConfigurationManager.ActiveCubePath = data.FilePath;
                    this.CubeDefEditor.Text = Encoding.UTF8.GetString(data.Contents);
                }
            }
            else if(action == SAVE_CUBE_DEF_AS)
            {
                byte[] contents = Encoding.UTF8.GetBytes(this.CubeDefEditor.Text);
                FileData data = await ConfigurationManager.FilePicker.SaveFileAs(contents, ConfigurationManager.ActiveDeck.Name + ".cdef");
                if(data is null)
                {
                    await DisplayAlert("Error", "Failed to save Cube Defintion", "Okay");
                }

                this.Path = ConfigurationManager.ActiveCubePath = data.FilePath;
            }
        }

        public void OnDefChanged(object sender, TextChangedEventArgs args)
        {
            byte[] contents = Encoding.UTF8.GetBytes(args.NewTextValue);
            ConfigurationManager.FilePicker.SaveFile(contents, this.Path);
        }
	}
}