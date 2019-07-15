using MtSparked.Interop.Models;
using MtSparked.Interop.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using MtSparked.Core;
using MtSparked.Interop.Utils;

namespace MtSparked.UI.ViewModels {
    public class DeckViewModel : Model {

        private ObservableCollection<Board> boards = new ObservableCollection<Board>();
        public ObservableCollection<Board> Boards {
            get { return this.boards; }
            set { _ = this.SetProperty(ref this.boards, value); }
        }

        public IDictionary<string, Board> BoardByName { get; } = new Dictionary<string, Board>();

        private Deck deck;
        public Deck Deck {
            get { return this.deck; }
            set {
                _ = this.SetProperty(ref this.deck, value);
                this.OnPropertyChanged(nameof(this.Name));
            }
        }

        public string Name => this.Deck?.Name;

        public DeckViewModel(Deck deck) {
            this.Deck = deck;

            this.UpdateBoards();

            this.Deck.ChangeEvent += this.UpdateBoards;
            ConfigurationManager.PropertyChanged += this.OnUniqueChanged;
        }
        
        public void UpdateBoards(object sender = null, DeckChangedEventArgs args = null) {
            if (args is NameChangedEventArgs nameChange) {
                this.OnPropertyChanged(nameof(this.Name));
            } else if (args is null ||
                     args is CardCountChangedEventArgs ||
                     args is BoardChangedEventArgs) {
                this.BoardByName.Clear();
                ObservableCollection<Board> boards = new ObservableCollection<Board>();
                foreach (Deck.BoardInfo boardInfo in this.Deck.BoardInfos) {
                    string name = boardInfo.Name;
                    if (!boardInfo.Visible) {
                        continue;
                    }

                    List<Deck.BoardItem> boardItems = new List<Deck.BoardItem>(this.Deck.Boards[name].Values);
                    IEnumerable<CardWithBoard> cards = boardItems.OrderBy(bi => bi.Card.Cmc).ThenBy(bi => bi.Card.Name)
                                                                       .Select(bi => new CardWithBoard(bi.Card, name, this.Deck));
                    if (ConfigurationManager.ShowUnique) {
                        cards = cards.DistinctBy(cwb => cwb.Name);
                    }
                    Board board = new Board(name + ": " + this.Deck.GetCountInBoard(name).ToString(), cards);
                    boards.Add(board);
                    this.BoardByName[name] = board;
                }
                this.Boards = boards;
            }
        }

        public void OnUniqueChanged(object sender, PropertyChangedEventArgs args) {
            if(args.PropertyName == nameof(ConfigurationManager.ShowUnique)) {
                this.UpdateBoards();
            }
        }

        public class Board : List<CardWithBoard> {

            public string Name { get; }

            public Board(string name, IEnumerable<CardWithBoard> cards)
            : base(cards) {
                this.Name = name;
            }

        }

        public class CardWithBoard {

            public string Id { get; }
            public string Name { get; }
            public string TypeLine { get; }
            public string ManaCost { get; }
            public string SetCode { get; }
            public string CroppedImageUrl { get; }
            public string ColorIdentity { get; }
            public string Rarity { get; }
            public Deck Deck { get; }
            public string Board { get; }

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

        }

    }
}
