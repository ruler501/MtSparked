using Gatherer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gatherer.ViewModels
{
    class DeckViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Board> boards = new ObservableCollection<Board>();
        public ObservableCollection<Board> Boards { get => boards; set => SetProperty(ref boards, value); }

        Deck Deck;

        string name = "Unnamed";
        public string Name { get => name; set => SetProperty(ref name, value); }

        public DeckViewModel(Deck deck)
        {
            this.Name = deck.Name;
            this.Deck = deck;

            this.UpdateBoards();

            this.Deck.ChangeEvent += this.UpdateBoards;
        }

        public void UpdateBoards(object sender = null, DeckChangedEventArgs args = null)
        {
            this.Boards.Clear();
            foreach (KeyValuePair<string, IDictionary<string, Deck.BoardItem>> pair in this.Deck.Boards)
            {
                string name = pair.Key;
                if (name == Deck.MASTER) continue;

                List<Deck.BoardItem> boardItems = new List<Deck.BoardItem>(pair.Value.Values);
                IEnumerable<CardWithBoard> cards = boardItems.OrderBy(bi => bi.Card.Cmc).ThenBy(bi => bi.Card.Name)
                                                                   .Select(bi => new CardWithBoard(bi.Card, name));
                this.Boards.Add(new Board(name, cards));
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public class Board : List<CardWithBoard>
        {
            public string Name { get; set; }

            public Board(string name, IEnumerable<CardWithBoard> cards)
            : base(cards)
            {
                this.Name = name;
            }
        }

        public class CardWithBoard
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string TypeLine { get; set; }
            public string ManaCost { get; set; }
            public string SetCode { get; set; }
            public string CroppedImageUrl { get; set; }
            public CardWithBoard(Card card, string board)
            {
                this.Id = card.Id;
                this.Name = card.Name;
                this.TypeLine = card.TypeLine;
                this.ManaCost = card.ManaCost;
                this.SetCode = card.SetCode;
                this.CroppedImageUrl = card.CroppedImageUrl;
                this.Board = board;
            }

            public string Board { get; set; }
        }
    }
}
