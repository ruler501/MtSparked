using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WeakEvent;

namespace MtSparked.Interop.Models {
    public class Deck : Model {

        public const string MASTER = "Total";
        public const string MAINBOARD = "Mainboard";
        public const string SIDEBOARD = "Sideboard";
        public const string UNNAMED = "Unnamed";
        public const string NAME = "Name";
        public const string VISIBLE = "Visible";
        public const string EDITABLE = "Editable";
        public const string BOARDS = "Boards";
        public const string ID = "Id";
        public const string FOIL = "Foil";
        public const string NORMAL = "Normal";
        public const string CARDS = "Cards";

        private string name = UNNAMED;
        public string Name {
            get { return this.name; }
            set {
                this.name = value;
                // Can't save deck here because of the constructor.
                // TODO: Fix this since the constructor will no longer call it.
                this.changeEventSource.Raise(this, new NameChangedEventArgs(value));
            }
        }

        public static Deck FromJdec(string defaultDeckPath) => throw new NotImplementedException();

        // TODO: Update all public set methods to conform to Model requirements.
        // TODO: Given how several of these are interrelated it should not be possible to modify one without also modifying the others.
        public bool AutoSave { get; set; }
        private string storagePath;
        public string StoragePath {
            get { return this.storagePath; }
            set {
                this.storagePath = value;
                this.changeEventSource.Raise(this, new PathChangedEventArgs(this.StoragePath));
            }
        }
        public IDictionary<string, IDictionary<string, BoardItem>> Boards { get; set; } = new Dictionary<string, IDictionary<string, BoardItem>> {
            [MASTER] = new Dictionary<string, BoardItem>(),
            [MAINBOARD] = new Dictionary<string, BoardItem>(),
            [SIDEBOARD] = new Dictionary<string, BoardItem>()
        };
        public IDictionary<string, ISet<string>> CardsByName { get; set; } = new Dictionary<string, ISet<string>>();
        public ObservableCollection<BoardInfo> BoardInfos { get; set; } = new ObservableCollection<BoardInfo>() {
                new BoardInfo(MASTER),
                new BoardInfo(MAINBOARD),
                new BoardInfo(SIDEBOARD)
            };

        public ICollection<string> BoardNames => this.Boards.Keys;
        public IDictionary<string, BoardItem> Master => this.Boards[MASTER];
        public ICollection<BoardItem> Cards => this.Master.Values;
        public ICollection<string> CardNames => this.CardsByName.Keys;

        private bool Loading = false;

        private readonly WeakEventSource<DeckChangedEventArgs> changeEventSource = new WeakEventSource<DeckChangedEventArgs>();
        public event EventHandler<DeckChangedEventArgs> ChangeEvent {
            add { this.changeEventSource.Subscribe(value); }
            remove { this.changeEventSource.Unsubscribe(value); }
        }

        public Deck(bool autoSave = true) {
            this.AutoSave = autoSave;
        }

        public void AddBoard(string name) {
            if (this.Boards.ContainsKey(name)) {
                return;
            }

            this.Boards[name] = new Dictionary<string, BoardItem>();
            this.BoardInfos.Add(new BoardInfo(name));
            this.changeEventSource.Raise(this, new BoardChangedEventArgs(name, true));
            /* TODO: Handle integration with file management.
            if(this.AutoSave) {
                this.SaveDeck();
            }
            */
        }

        public void RemoveBoard(string name)
        {
            if (name is null || !Boards.ContainsKey(name) || name == MASTER) {
                return;
            }

            _ = this.Boards.Remove(name);
            for(int i=0; i < this.BoardInfos.Count; i++) {
                if(this.BoardInfos[i].Name == name) {
                    this.BoardInfos.RemoveAt(i);
                    break;
                }
            }
            this.changeEventSource.Raise(this, new BoardChangedEventArgs(name, false));
            /* TODO: Handle integration with file management.
            if(this.AutoSave) {
                this.SaveDeck();
            }
            */
        }

        public void AddCard(Card card, string boardName=MASTER, bool normal = true, int amount=1) {
            if(!this.Boards.ContainsKey(boardName)) {
                this.AddBoard(boardName);
            }
            IDictionary<string, BoardItem> board = this.Boards[boardName];

            BoardItem boardItem;
            if (board.ContainsKey(card.Id)) {
                boardItem = board[card.Id];
            } else {
                boardItem = new BoardItem() { Card = card };
                board[card.Id] = boardItem;
                if (this.CardsByName.ContainsKey(card.Name)) {
                    this.CardsByName[card.Name].Add(card.Id);
                } else {
                    this.CardsByName.Add(card.Name, new HashSet<string> { card.Id });
                }
            }

            if (normal) {
                boardItem.NormalCount += amount;
            } else {
                boardItem.FoilCount += amount;
            }

            if (!this.Master.ContainsKey(card.Id)) {
                this.Master[card.Id] = boardItem.Copy();
            } else if(this.Master[card.Id].NormalCount < boardItem.NormalCount) {
                this.Master[card.Id].NormalCount = boardItem.NormalCount;
            } else if (this.Master[card.Id].FoilCount < boardItem.FoilCount) {
                this.Master[card.Id].FoilCount = boardItem.FoilCount;
            }

            this.changeEventSource.Raise(this, new CardCountChangedEventArgs(card, boardName, normal, amount));
            /* TODO: Handle integration with file management.
            if(this.AutoSave) {
                this.SaveDeck();
            }
            */
        }

        public bool RemoveCard(Card card, string boardName=MASTER, bool normal = true, int amount = 1) {
            if (!this.Boards.ContainsKey(boardName)) {
                return false;
            }
            IDictionary<string, BoardItem> board = this.Boards[boardName];

            BoardItem boardItem;
            if (board.ContainsKey(card.Id)) {
                boardItem = board[card.Id];
            } else {
                return false;
            }

            if (normal) {
                if (boardItem.NormalCount == 0) {
                    return false;
                }
                boardItem.NormalCount -= amount;
            } else {
                if (boardItem.FoilCount == 0) {
                    return false;
                }
                boardItem.FoilCount -= amount;
            }

            if (boardName == MASTER) {
                int masterNormalCount = boardItem.NormalCount;
                int masterFoilCount = boardItem.FoilCount;
                foreach(KeyValuePair<string, IDictionary<string, BoardItem>> pair in this.Boards) {
                    IDictionary<string, BoardItem> cards = pair.Value;
                    if (pair.Key == MASTER || !cards.ContainsKey(card.Id)) {
                        continue;
                    }

                    BoardItem otherBoardItem = cards[card.Id];
                    otherBoardItem.NormalCount = Math.Min(otherBoardItem.NormalCount, masterNormalCount);
                    otherBoardItem.FoilCount = Math.Min(otherBoardItem.FoilCount, masterFoilCount);

                    if(otherBoardItem.NormalCount == 0 && otherBoardItem.FoilCount == 0) {
                        cards.Remove(card.Id);
                    }
                }
            }

            if(boardItem.NormalCount == 0 && boardItem.FoilCount == 0) {
                board.Remove(card.Id);
                if (boardName == MASTER) {
                    _ = this.CardsByName[card.Name].Remove(card.Id);
                    if (this.CardsByName[card.Name].Count == 0) {
                        _ = this.CardsByName.Remove(card.Name);
                    }
                }
            }

            this.changeEventSource.Raise(this, new CardCountChangedEventArgs(card, boardName, normal, -amount));
            /* TODO: Handle integration with file management.
            if(this.AutoSave) {
                this.SaveDeck();
            }
            */
            return true;
        }

        public int GetNormalCount(Card card, string boardName = MASTER) => this.GetCountInternal(card.Id, boardName, bi => bi.NormalCount);

        public int GetFoilCount(Card card, string boardName = MASTER) => this.GetCountInternal(card.Id, boardName, bi => bi.FoilCount);

        public int GetCount(string id, string boardName = MASTER) => this.GetCountInternal(id, boardName, bi => bi.Count);

        public int GetCountByName(string name, string boardName = MASTER) => this.GetCountByNameInternal(name, boardName, bi => bi.Count);

        public int GetNormalCountByName(Card card, string boardName = MASTER) => this.GetCountByNameInternal(card.Name, boardName, bi => bi.NormalCount);

        public int GetFoilCountByName(Card card, string boardName = MASTER) => this.GetCountByNameInternal(card.Name, boardName, bi => bi.FoilCount);

        public int GetCountInBoard(string name) => this.GetCountInBoardInternal(name, bi => bi.Count);

        private int GetCountInternal(string id, string boardName, Func<BoardItem, int> toCount) {
            if (!this.BoardNames.Contains(boardName) || !this.Boards[boardName].ContainsKey(id)) {
                return 0;
            }
            return toCount(this.Boards[boardName][id]);
        }

        private int GetCountByNameInternal(string name, string boardName, Func<BoardItem, int> toCount) {
            if (!this.BoardNames.Contains(boardName) || !this.CardNames.Contains(name)) {
                return 0;
            }
            IDictionary<string, BoardItem> board = this.Boards[boardName];
            ISet<string> ids = this.CardsByName[name];
            int sum = 0;
            foreach (string id in ids) {
                if (board.ContainsKey(id)) {
                    sum += toCount(board[id]);
                }
            }
            return sum;
        }

        private int GetCountInBoardInternal(string name, Func<BoardItem, int> toCount) {
            if (!this.BoardNames.Contains(name)) {
                return 0;
            }

            int sum = 0;
            foreach(BoardItem boardItem in this.Boards[name].Values) {
                sum += toCount(boardItem);
            }
            return sum;
        }

        // TODO: Should we just call this from the modification methods?
        public void BoardInfoRefreshed() {
            this.changeEventSource.Raise(this, new BoardChangedEventArgs(null, false));
            /* TODO: Handle integration with file management.
            if(this.AutoSave) {
                this.SaveDeck();
            }
            */
        }

        public class BoardItem
        {

            public Card Card { get; set; }
            public int FoilCount { get; set; }
            public int NormalCount { get; set; }

            public int Count => this.NormalCount + this.FoilCount;

            public BoardItem Copy() {
                return new BoardItem() {
                    Card = Card,
                    FoilCount = FoilCount,
                    NormalCount = NormalCount
                };
            }

        }

        public class BoardInfo : Model
        {

            private string name;
            public string Name {
                get { return this.name; }
                set { _ = this.SetProperty(ref this.name, value); }
            }
            private bool viewable; 
            public bool Visibible {
                get { return this.viewable; }
                set { _ = this.SetProperty(ref this.viewable, value); }
            }
            private bool editable;
            public bool Editable {
                get { return this.editable; }
                set { _ = this.SetProperty(ref this.editable, value); }
            }

            public BoardInfo(string name)
                : this(name, true, true)
            {}

            public BoardInfo(string name, bool viewable, bool editable) {
                this.name = name;
                this.viewable = viewable;
                this.editable = editable;
            }

        }

    }

    public class DeckChangedEventArgs : EventArgs
    {}

    public class CardCountChangedEventArgs : DeckChangedEventArgs {
        public CardCountChangedEventArgs(Card card, string board, bool normal, int amount) {
            this.Card = card;
            this.Board = board;
            this.Amount = amount;
            this.Normal = normal;
        }

        public Card Card { get; }
        public int Amount { get; }
        public bool Normal { get; }
        public string Board { get; }

    }

    public class BoardChangedEventArgs : DeckChangedEventArgs {


        public BoardChangedEventArgs(string board, bool added) {
            this.Board = board;
            this.Added = added;
        }

        public string Board { get; }
        public bool Added { get; }

    }

    public class PathChangedEventArgs : DeckChangedEventArgs
    {

        public PathChangedEventArgs(string path) {
            this.Path = path;
        }

        public string Path { get; }

    }

    public class NameChangedEventArgs : DeckChangedEventArgs
    {

        public NameChangedEventArgs(string name) {
            this.Name = name;
        }

        public string Name { get; }

    }
}
