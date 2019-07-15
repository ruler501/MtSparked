using MtSparked.Interop.Models;
using MtSparked.Interop.Services;
using MtSparked.UI.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.UI.Views {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CardItemView : ViewCell {

        public CardItemView () {
            this.InitializeComponent ();
            this.UpdateCount();
            ConfigurationManager.ActiveDeck.ChangeEvent += this.UpdateCount;
		}

        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();

            this.Image.Source = null;
            if (this.BindingContext is null) {
                return;
            }

            string colorIdentity = "";
            if (this.BindingContext is Card card) {
                this.Image.Source = card.CroppedImageUrl;
                colorIdentity = card.ColorIdentity;
            } else if (this.BindingContext is DeckViewModel.CardWithBoard cwb) {
                this.Image.Source = cwb.CroppedImageUrl;
                colorIdentity = cwb.ColorIdentity;
            }
            XFGloss.Gradient colorGradient = this.ColorGradient;
            colorGradient.Steps.Clear();
            double colors = 0.0;
            double div = colorIdentity.Length > 1 ? colorIdentity.Length - 1 : 1.0;
            Color lastColor = new Color(225 / 255.0, 228 / 255.0, 233 / 255.0);
            // TODO #81: Simplify Mapping from ColorIdentity to Gradient
            if (colorIdentity.Contains("W")) {
                lastColor = new Color(242 / 255.0, 226 / 255.0, 182 / 255.0);
                colorGradient.AddStep(lastColor, colors++ / div);
            }
            if (colorIdentity.Contains("U")) {
                lastColor = new Color(182 / 255.0, 215 / 255.0, 242 / 255.0);
                colorGradient.AddStep(lastColor, colors++ / div);
            }
            if (colorIdentity.Contains("B")) {
                lastColor = new Color(153 / 255.0, 141 / 255.0, 138 / 255.0);
                colorGradient.AddStep(lastColor, colors++ / div);
            }
            if (colorIdentity.Contains("R")) {
                lastColor = new Color(242 / 255.0, 155 / 255.0, 121 / 255.0);
                colorGradient.AddStep(lastColor, colors++ / div);
            }
            if (colorIdentity.Contains("G")) {
                lastColor = new Color(163 / 255.0, 217 / 255.0, 184 / 255.0);
                colorGradient.AddStep(lastColor, colors++ / div);
            }
            if(colorIdentity.Length == 0) {
                lastColor = new Color(225 / 255.0, 228 / 255.0, 230 / 255.0);
                colorGradient.AddStep(lastColor, 0);
            }
            if (colorIdentity.Length <= 1) {
                colorGradient.AddStep(lastColor, 1);
            }

            this.UpdateCount();
        }

        public void UpdateCount(object sender=null, DeckChangedEventArgs args = null) {
            if (this.BindingContext is null) {
                return;
            }
            string board = Deck.MASTER;
            string id = null;
            string name = null;
            Deck deck = ConfigurationManager.ActiveDeck;
            if (this.BindingContext is DeckViewModel.CardWithBoard cwb) {
                board = cwb.Board;
                id = cwb.Id;
                name = cwb.Name;
                deck = cwb.Deck;
            } else if(this.BindingContext is Card c) {
                id = c.Id;
                name = c.Name;
            } if (ConfigurationManager.ShowUnique) {
                this.Count.Text = deck.GetCountByName(name, board).ToString();
            } else {
                this.Count.Text = deck.GetCount(id, board).ToString();
            }
        }

    }
}