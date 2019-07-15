using MtSparked.Interop.Services;
using MtSparked.Interop.Models;
using System.Collections.ObjectModel;

namespace MtSparked.UI.Models {
    public class DomainCriteria : Model {
		
        public const string ALL_CARDS = "All Cards";
        public const string ACTIVE_DECK = "Active Deck";
        private static readonly string[] DEFAULT_FIELDS = new[] { ALL_CARDS, ACTIVE_DECK };
        private ObservableCollection<string> fields = new ObservableCollection<string>(DEFAULT_FIELDS);
        public ObservableCollection<string> Fields {
            get { return this.fields; }
            set { _ = this.SetProperty(ref this.fields, value); }
        }
        
        public string Field { get; set; } = "Name";
        public bool Set { get; set; } = true;

        public DomainCriteria() {
            this.PopulateFields(null, new BoardChangedEventArgs(null, false));
        }

        public void PopulateFields(object sender, DeckChangedEventArgs args) {
            if (args is BoardChangedEventArgs) {
                this.fields = new ObservableCollection<string>(DEFAULT_FIELDS);
                foreach (Deck.BoardInfo info in ConfigurationManager.ActiveDeck.BoardInfos) {
                    if (!info.Visible) {
						continue;
					}
                    string name = info.Name;
                    this.fields.Add(name);
                }
            }
        }
		
    }
}
