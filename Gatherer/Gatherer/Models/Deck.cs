using Gatherer.FilePicker;
using Gatherer.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using WeakEvent;

namespace Gatherer.Models
{
    public class Deck
    {
        public const string MASTER = "Total";

        private string name;
        public string Name
        {
            get => name;
            set
            {
                this.name = value;
                this.SaveDeck();
                this.changeEventSource.Raise(this, new NameChangedEventArgs(value));
            }
        }
        public string StoragePath { get; set; }
        public IDictionary<string, IDictionary<string, BoardItem>> Boards;
        public IDictionary<string, ISet<string>> CardsByName { get; set; }

        public ICollection<string> BoardNames => this.Boards.Keys;
        public IDictionary<string, BoardItem> Master => this.Boards[MASTER];
        public ICollection<BoardItem> Cards => this.Master.Values;
        public ICollection<string> CardNames => this.CardsByName.Keys;

        private bool Loading = false;


        private readonly WeakEventSource<DeckChangedEventArgs> changeEventSource = new WeakEventSource<DeckChangedEventArgs>();

        public event EventHandler<DeckChangedEventArgs> ChangeEvent
        {
            add { changeEventSource.Subscribe(value); }
            remove { changeEventSource.Unsubscribe(value); }
        }

        public Deck()
        {
            this.Boards = new Dictionary<string, IDictionary<string, BoardItem>>
            {
                [MASTER] = new Dictionary<string, BoardItem>(),
                ["Mainboard"] = new Dictionary<string, BoardItem>(),
                ["Sideboard"] = new Dictionary<string, BoardItem>()
            };

            this.CardsByName = new Dictionary<string, ISet<string>>();
        }

        public Deck(string path)
        {

            this.CardsByName = new Dictionary<string, ISet<string>>();
            if (ConfigurationManager.FilePicker.PathExists(path))
            {
                Loading = true;
                this.Boards = new Dictionary<string, IDictionary<string, BoardItem>>()
                {
                    [MASTER] = new Dictionary<string, BoardItem>()
                };

                byte[] fileData = ConfigurationManager.FilePicker.OpenFile(path);

                using (MemoryStream file = new MemoryStream(fileData))
                using (StreamReader textFile = new StreamReader(file, Encoding.UTF8))
                using (JsonTextReader reader = new JsonTextReader(textFile))
                {
                    JObject deck = (JObject)JToken.ReadFrom(reader);

                    this.Name = (string)deck["Name"];
                    JArray boardNames = (JArray)deck["BoardNames"];
                    foreach (string boardName in boardNames)
                    {
                        this.Boards[boardName] = new Dictionary<string, BoardItem>();
                    }

                    JArray cards = (JArray)deck["Cards"];
                    foreach (JObject card in cards)
                    {
                        string cardId = (string)card["CardId"];

                        Card cardValue = CardDataStore.ById(cardId);

                        JArray boards = (JArray)card["Boards"];
                        // Must process Master first to prevent duplication
                        foreach (JObject board in boards.OrderBy(b => (string)b["BoardName"] != MASTER))
                        {
                            string boardName = (string)board["BoardName"];
                            int normalCount = (int)board["NormalCount"];
                            int foilCount = (int)board["FoilCount"];

                            this.AddCard(cardValue, boardName, true, normalCount);
                            this.AddCard(cardValue, boardName, false, foilCount);
                        }
                    }
                }
                this.StoragePath = path;
                Loading = false;
            }
            else
            {
                this.Boards = new Dictionary<string, IDictionary<string, BoardItem>>
                {
                    [MASTER] = new Dictionary<string, BoardItem>(),
                    ["Mainboard"] = new Dictionary<string, BoardItem>(),
                    ["Sideboard"] = new Dictionary<string, BoardItem>()
                };
                this.Name = "Unnamed";
            }
        }

        public async void SaveDeckAs()
        {
            if (Loading || this.StoragePath is null) return;

            string text = this.ToString();
            byte[] fileContents = Encoding.UTF8.GetBytes(text);

            FileData file = await ConfigurationManager.FilePicker.SaveFileAs(fileContents, this.Name + ".jdec");
            if(file is null)
            {
                // Note failed save
                return;
            }

            if(this.StoragePath != file.FilePath)
            {
                ConfigurationManager.FilePicker.ReleaseFile(this.StoragePath);
                this.StoragePath = file.FilePath;
            }
            
            this.changeEventSource.Raise(this, new PathChangedEventArgs(this.StoragePath));
        }

        public void SaveDeck()
        {
            if (Loading || this.StoragePath is null) return;

            string text = this.ToString();
            byte[] fileContents = Encoding.UTF8.GetBytes(text);

            ConfigurationManager.FilePicker.SaveFile(fileContents, this.StoragePath);
        }

        public override string ToString()
        {
            JObject result = new JObject
            {
                ["Name"] = this.Name,
                ["BoardNames"] = new JArray(this.BoardNames)
            };
            JArray cards = new JArray();
            foreach(BoardItem bi in this.Boards[MASTER].Values)
            {
                string cardId = bi.Card.Id;
                JObject card = new JObject
                {
                    ["CardId"] = cardId,
                    ["CardName"] = bi.Card.Name
                };

                JArray boards = new JArray();

                foreach(KeyValuePair<string, IDictionary<string, BoardItem>> pair in this.Boards.OrderBy(p => p.Key))
                {
                    string name = pair.Key;
                    IDictionary<string, BoardItem> boardValue = pair.Value;
                    if (boardValue.ContainsKey(cardId))
                    {
                        int normalCount = boardValue[cardId].NormalCount;
                        int foilCount = boardValue[cardId].FoilCount;

                        JObject board = new JObject
                        {
                            ["BoardName"] = name,
                            ["NormalCount"] = normalCount,
                            ["FoilCount"] = foilCount
                        };
                        boards.Add(board);
                    }
                }
                card["Boards"] = boards;
                cards.Add(card);
            }
            result["Cards"] = cards;
            
            return result.ToString();
        }

        public void AddBoard(string name)
        {
            if (Boards.ContainsKey(name))
            {
                return;
            }

            this.Boards[name] = new Dictionary<string, BoardItem>();
            this.changeEventSource.Raise(this, new BoardChangedEventArgs(name, true));
            this.SaveDeck();
        }

        public void RemoveBoard(string name)
        {
            if (Boards.ContainsKey(name) && name != MASTER)
            {
                this.Boards.Remove(name);
            }
            this.changeEventSource.Raise(this, new BoardChangedEventArgs(name, false));
            this.SaveDeck();
        }

        public void AddCard(Card card, string boardName=MASTER, bool normal = true, int amount=1)
        {
            if(!Boards.ContainsKey(boardName))
            {
                this.AddBoard(boardName);
            }
            IDictionary<string, BoardItem> board = Boards[boardName];

            BoardItem boardItem;
            if (board.ContainsKey(card.Id))
            {
                boardItem = board[card.Id];
            }
            else
            {
                boardItem = new BoardItem()
                {
                    Card = card
                };
                board[card.Id] = boardItem;
                if (CardsByName.ContainsKey(card.Name))
                {
                    CardsByName[card.Name].Add(card.Id);
                }
                else
                {
                    CardsByName.Add(card.Name, new HashSet<string> { card.Id });
                }
            }

            if (normal) boardItem.NormalCount += amount;
            else boardItem.FoilCount += amount;

            if (!Master.ContainsKey(card.Id))
            {
                Master[card.Id] = boardItem.Copy();
            }
            else if(Master[card.Id].NormalCount < boardItem.NormalCount)
            {
                Master[card.Id].NormalCount = boardItem.NormalCount;
            }
            else if (Master[card.Id].FoilCount < boardItem.FoilCount)
            {
                Master[card.Id].FoilCount = boardItem.FoilCount;
            }

            changeEventSource.Raise(this, new CardCountChangedEventArgs(card, boardName, normal, amount));

            this.SaveDeck();
        }

        public void RemoveCard(Card card, string boardName=MASTER, bool normal = true, int amount = 1)
        {
            if (!Boards.ContainsKey(boardName))
            {
                return;
            }
            IDictionary<string, BoardItem> board = Boards[boardName];

            BoardItem boardItem;
            if (board.ContainsKey(card.Id))
            {
                boardItem = board[card.Id];
            }
            else
            {
                return;
            }

            if (normal) boardItem.NormalCount -= amount;
            else boardItem.FoilCount -= amount;

            if(boardItem.NormalCount < 0) boardItem.NormalCount = 0;
            if (boardItem.FoilCount < 0) boardItem.FoilCount = 0;

            if(boardName == "Master")
            {
                foreach(KeyValuePair<string, IDictionary<string, BoardItem>> pair in Boards)
                {
                    if (pair.Key == "Master") continue;

                    IDictionary<string, BoardItem> cards = pair.Value;
                    if (!cards.ContainsKey(card.Id)) continue;

                    BoardItem otherBoardItem = cards[card.Id];
                    if(otherBoardItem.NormalCount > boardItem.NormalCount)
                    {
                        otherBoardItem.NormalCount = boardItem.NormalCount;
                    }
                    else if(otherBoardItem.FoilCount > boardItem.FoilCount)
                    {
                        otherBoardItem.FoilCount = boardItem.FoilCount;
                    }

                    if(otherBoardItem.NormalCount == 0 && otherBoardItem.FoilCount == 0)
                    {
                        cards.Remove(card.Id);
                    }
                }
            }

            if(boardItem.NormalCount == 0 && boardItem.FoilCount == 0)
            {
                board.Remove(card.Id);
                if (boardName == "Master")
                {
                    CardsByName[card.Name].Remove(card.Id);
                    if (CardsByName[card.Name].Count == 0)
                    {
                        CardsByName.Remove(card.Name);
                    }
                }
            }

            changeEventSource.Raise(this, new CardCountChangedEventArgs(card, boardName, normal, -amount));

            this.SaveDeck();
        }

        public int GetNormalCount(Card card, string boardName=MASTER)
        {
            return this.GetCountInternal(card.Id, boardName, bi => bi.NormalCount);
        }

        public int GetFoilCount(Card card, string boardName=MASTER)
        {
            return this.GetCountInternal(card.Id, boardName, bi => bi.FoilCount);
        }

        public int GetCount(string id, string boardName = MASTER)
        {
            return this.GetCountInternal(id, boardName, bi => bi.Count);
        }

        private int GetCountInternal(string id, string boardName, Func<BoardItem, int> toCount)
        {
            if (!this.BoardNames.Contains(boardName) || !this.Boards[boardName].ContainsKey(id))
            {
                return 0;
            }
            return toCount(this.Boards[boardName][id]);
        }

        private int GetCountByNameInternal(string name, string boardName, Func<BoardItem, int> toCount)
        {
            if (!this.BoardNames.Contains(boardName) || !this.CardNames.Contains(name))
            {
                return 0;
            }
            IDictionary<string, BoardItem> board = this.Boards[boardName];
            ISet<string> ids = this.CardsByName[name];
            int sum = 0;
            foreach(string id in ids)
            {
                if (board.ContainsKey(id))
                {
                    sum += toCount(board[id]);
                }
            }
            return sum;
        }

        public int GetCountByName(string id, string boardName = MASTER)
        {
            return this.GetCountByNameInternal(id, boardName, bi => bi.Count);
        }

        public int GetNormalCountByName(Card card, string boardName = MASTER)
        {
            return this.GetCountByNameInternal(card.Id, boardName, bi => bi.NormalCount);
        }

        public int GetFoilCountByName(Card card, string boardName = MASTER)
        {
            return this.GetCountInternal(card.Id, boardName, bi => bi.FoilCount);
        }

        public class BoardItem
        {
            public Card Card { get; set; }
            public int FoilCount { get; set; }
            public int NormalCount { get; set; }

            public int Count => NormalCount + FoilCount;

            public BoardItem Copy()
            {
                return new BoardItem()
                {
                    Card = Card,
                    FoilCount = FoilCount,
                    NormalCount = NormalCount
                };
            }
        }
    }

    public class DeckChangedEventArgs : EventArgs
    {
    }

    public class CardCountChangedEventArgs : DeckChangedEventArgs
    {
        private Card card;
        private int amount;
        private bool normal;
        private string board;

        public CardCountChangedEventArgs(Card card, string board, bool normal, int amount)
        {
            this.card = card;
            this.board = board;
            this.amount = amount;
            this.normal = normal;
        }

        public Card Card => card;
        public int Amount => amount;
        public bool Normal => normal;
        public string Board => board;
    }

    public class BoardChangedEventArgs : DeckChangedEventArgs
    {
        private string board;
        private bool added;

        public BoardChangedEventArgs(string board, bool added)
        {
            this.board = board;
            this.added = added;
        }

        public string Board => this.board;
        public bool Added => this.Added;
    }

    public class PathChangedEventArgs : DeckChangedEventArgs
    {
        private string path;

        public PathChangedEventArgs(string path)
        {
            this.path = path;
        }

        public string Path => path;
    }

    public class NameChangedEventArgs : DeckChangedEventArgs
    {
        private string name;

        public NameChangedEventArgs(string name)
        {
            this.name = name;
        }

        public string Name => name;
    }
}
