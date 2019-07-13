using MtSparked.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MtSparked.Models
{
    public class DomainCriteria : Model
    {
        public const string ALL_CARDS = "All Cards";
        public const string ACTIVE_DECK = "Active Deck";
        private static readonly string[] DEFAULT_FIELDS = new[] { ALL_CARDS, ACTIVE_DECK };
        private ObservableCollection<string> fields = new ObservableCollection<string>(DEFAULT_FIELDS);
        public ObservableCollection<string> Fields
        {
            get => fields;
            set => SetProperty(ref fields, value);
        }
        
        public string Field { get; set; }
        public bool Set { get; set; }

        public DomainCriteria()
        {
            this.Field = "Name";
            this.Set = true;

            this.PopulateFields(null, new BoardChangedEventArgs(null, false));
        }

        public void PopulateFields(object sender, DeckChangedEventArgs args)
        {
            if (args is BoardChangedEventArgs)
            {
                this.fields = new ObservableCollection<string>(DEFAULT_FIELDS);
                foreach (Deck.BoardInfo info in ConfigurationManager.ActiveDeck.BoardInfos)
                {
                    if (!info.Viewable) continue;
                    string name = info.Name;
                    this.fields.Add(name);
                }
            }
        }
    }
}
