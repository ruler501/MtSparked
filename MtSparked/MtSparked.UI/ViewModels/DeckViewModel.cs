using MtSparked.Models;
using MtSparked.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MtSparked.ViewModels
{
    class DeckViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Board> boards = new ObservableCollection<Board>();
        public ObservableCollection<Board> Boards { get => boards; set => SetProperty(ref boards, value); }

        public IDictionary<string, Board> BoardByName = new Dictionary<string, Board>();

        Deck Deck;

        string name = "Unnamed";
        public string Name { get => name; set => SetProperty(ref name, value); }

        public DeckViewModel(Deck deck)
        {
            this.Name = deck.Name;
            this.Deck = deck;

            this.UpdateBoards();

            this.Deck.ChangeEvent += this.UpdateBoards;
            ConfigurationManager.PropertyChanged += this.OnUniqueChanged;
        }
        
        public void UpdateBoards(object sender = null, DeckChangedEventArgs args = null)
        {
            if (args is NameChangedEventArgs nameChange)
            {
                this.Name = nameChange.Name;
            }
            else if (args is null ||
                     args is CardCountChangedEventArgs ||
                     args is BoardChangedEventArgs)
            {
                this.BoardByName.Clear();
                ObservableCollection<Board> boards = new ObservableCollection<Board>();
                foreach (Deck.BoardInfo boardInfo in this.Deck.BoardInfos)
                {
                    string name = boardInfo.Name;
                    if (!boardInfo.Viewable) continue;

                    List<Deck.BoardItem> boardItems = new List<Deck.BoardItem>(this.Deck.Boards[name].Values);
                    IEnumerable<CardWithBoard> cards = boardItems.OrderBy(bi => bi.Card.Cmc).ThenBy(bi => bi.Card.Name)
                                                                       .Select(bi => new CardWithBoard(bi.Card, name, this.Deck));
                    if (ConfigurationManager.ShowUnique)
                    {
                        cards = cards.DistinctBy(cwb => cwb.Name);
                    }
                    Board board = new Board(name + ": " + this.Deck.GetCountInBoard(name).ToString(), cards);
                    boards.Add(board);
                    this.BoardByName[name] = board;
                }
                this.Boards = boards;
            }
        }

        public void OnUniqueChanged(object sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName == nameof(ConfigurationManager.ShowUnique))
            {
                this.UpdateBoards();
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
            public string ColorIdentity { get; set; }
            public string Rarity { get; set; }
            public Deck Deck { get; set; }

            public CardWithBoard(Card card, string board, Deck deck)
            {
                this.Id = card.Id;
                this.Name = card.Name;
                this.TypeLine = card.TypeLine;
                this.ManaCost = card.ManaCost;
                this.SetCode = card.SetCode;
                this.CroppedImageUrl = card.CroppedImageUrl;
                this.ColorIdentity = card.ColorIdentity;
                this.Rarity = card.Rarity;
                this.Board = board;
                this.Deck = deck;
            }

            public string Board { get; set; }
        }
    }
}
