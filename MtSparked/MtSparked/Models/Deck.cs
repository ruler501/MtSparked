using MtSparked.FilePicker;
using MtSparked.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WeakEvent;

namespace MtSparked.Models
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
                // Can't save deck here because of the constructor.
                this.changeEventSource.Raise(this, new NameChangedEventArgs(value));
            }
        }
        public bool AutoSave { get; set; }
        public string StoragePath { get; set; }
        public IDictionary<string, IDictionary<string, BoardItem>> Boards { get; set; }
        public IDictionary<string, ISet<string>> CardsByName { get; set; }
        public ObservableCollection<BoardInfo> BoardInfos { get; set; }

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

        public Deck(bool autoSave = true)
        {
            this.AutoSave = autoSave;
            this.Boards = new Dictionary<string, IDictionary<string, BoardItem>>
            {
                [MASTER] = new Dictionary<string, BoardItem>(),
                ["Mainboard"] = new Dictionary<string, BoardItem>(),
                ["Sideboard"] = new Dictionary<string, BoardItem>()
            };
            this.BoardInfos = new ObservableCollection<BoardInfo>()
            {
                new BoardInfo(MASTER),
                new BoardInfo("Mainboard"),
                new BoardInfo("Sideboard")
            };
            this.Name = "Unnamed";

            this.CardsByName = new Dictionary<string, ISet<string>>();
        }

        public static Deck FromJdec(string path)
        {
            Deck self = new Deck();
            if (ConfigurationManager.FilePicker.PathExists(path))
            {
                try
                {
                    self.Loading = true;
                    self.Boards = new Dictionary<string, IDictionary<string, BoardItem>>()
                    {
                        [MASTER] = new Dictionary<string, BoardItem>()
                    };
                    self.BoardInfos = new ObservableCollection<BoardInfo>();

                    byte[] fileData = ConfigurationManager.FilePicker.OpenFile(path);

                    using (MemoryStream file = new MemoryStream(fileData))
                    using (StreamReader textFile = new StreamReader(file, Encoding.UTF8))
                    using (JsonTextReader reader = new JsonTextReader(textFile))
                    {
                        JObject deck = (JObject)JToken.ReadFrom(reader);

                        self.Name = (string)deck["Name"];
                        JArray boardInfos = (JArray)deck["Boards"];
                        bool containedMaster = false;
                        foreach (JObject boardInfo in boardInfos)
                        {
                            string name = (string)boardInfo["Name"];
                            bool viewable = (bool)boardInfo["Viewable"];
                            bool editable = (bool)boardInfo["Editable"];
                            self.BoardInfos.Add(new BoardInfo(name, viewable, editable));
                            self.Boards[name] = new Dictionary<string, BoardItem>();
                            containedMaster |= name == MASTER;
                        }
                        if (!containedMaster)
                        {
                            self.BoardInfos.Add(new BoardInfo(MASTER));
                        }

                        JArray cards = (JArray)deck["Cards"];
                        foreach (JObject card in cards)
                        {
                            string cardId = (string)card["Id"];

                            Card cardValue = CardDataStore.ById(cardId);

                            JArray boards = (JArray)card["Boards"];
                            // Must process Master first to prevent duplication
                            foreach (JObject board in boards.OrderBy(b => (string)b["Name"] != MASTER))
                            {
                                string boardName = (string)board["Name"];
                                int normalCount = (int)board["Normal"];
                                int foilCount = (int)board["Foil"];

                                self.AddCard(cardValue, boardName, true, normalCount);
                                self.AddCard(cardValue, boardName, false, foilCount);
                            }
                        }
                    }
                    self.StoragePath = path;
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc);
                    self = new Deck();
                }
                finally
                {
                    self.Loading = false;
                }
            }

            return self;
        }

        public static Deck FromDec(string path)
        {
            Deck self = new Deck
            {
                Name = "Unnamed",
                StoragePath = ConfigurationManager.DefaultDeckPath
            };
            if (ConfigurationManager.FilePicker.PathExists(path))
            {
                try
                {
                    self.Boards = new Dictionary<string, IDictionary<string, BoardItem>>()
                    {
                        [MASTER] = new Dictionary<string, BoardItem>()
                    };
                    self.BoardInfos = new ObservableCollection<BoardInfo> { new BoardInfo(MASTER) };

                    byte[] fileData = ConfigurationManager.FilePicker.OpenFile(path);

                    string data = Encoding.UTF8.GetString(fileData);

                    string[] lines = data.Split('\n');
                    foreach (string linePreTrim in lines)
                    {
                        string line = linePreTrim.Trim();
                        const string MVID = "mvid:";
                        const string QTY = "qty:";
                        const string LOC = "loc:";
                        int mvidIndex = line.IndexOf(MVID);
                        int qtyIndex = line.IndexOf(QTY);
                        int locIndex = line.IndexOf(LOC);
                        if (mvidIndex < 0 || qtyIndex < 0 || locIndex < 0)
                        {
                            continue;
                        }

                        int nextSpaceIndex = line.IndexOf(' ', mvidIndex);
                        string mvid = line.Substring(mvidIndex + MVID.Length,
                                                     nextSpaceIndex - mvidIndex - MVID.Length);

                        nextSpaceIndex = line.IndexOf(' ', qtyIndex);
                        string qty = line.Substring(qtyIndex + QTY.Length,
                                                    nextSpaceIndex - qtyIndex - QTY.Length);

                        string loc = line.Substring(locIndex + LOC.Length);

                        Card card = CardDataStore.ByMvid(mvid);
                        if(card is null)
                        {
                            continue;
                        }

                        int count = Int32.Parse(qty);

                        if (loc == MASTER) loc = MASTER + "-1";

                        self.AddCard(card, loc, true, count);
                    }
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc);
                    self = new Deck();
                }
                finally
                {
                    self.Loading = false;
                }
            }
            self.Loading = false;
            return self;
        }

        public async void SaveDeckAs()
        {
            if (Loading) return;

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

        public async void SaveAsDec()
        {
            if (Loading) return;

            string text = this.AsDec();
            byte[] fileContents = Encoding.UTF8.GetBytes(text);

            FileData file = await ConfigurationManager.FilePicker.SaveFileAs(fileContents, this.Name + ".dec");
            if (file is null)
            {
                // Note failed save
                return;
            }
        }

        public void SaveTempDec()
        {
            if (Loading) return;

            string text = this.AsDec();

            byte[] fileContents = Encoding.UTF8.GetBytes(text);

            ConfigurationManager.FilePicker.SaveFile(fileContents, ConfigurationManager.DefaultTempDecPath);
        }

        public void SaveDeck()
        {
            if (Loading) return;

            if (this.StoragePath is null) this.StoragePath = ConfigurationManager.DefaultDeckPath;

            string text = this.ToString();

            byte[] fileContents = Encoding.UTF8.GetBytes(text);

            ConfigurationManager.FilePicker.SaveFile(fileContents, this.StoragePath);
        }

        public override string ToString()
        {
            JObject result = new JObject
            {
                ["Name"] = this.Name,
            };
            JArray boardInfos = new JArray();
            foreach(BoardInfo info in this.BoardInfos)
            {
                boardInfos.Add(new JObject
                {
                    ["Name"] = info.Name,
                    ["Viewable"] = info.Viewable,
                    ["Editable"] = info.Editable
                });
            }
            result["Boards"] = boardInfos;

            JArray cards = new JArray();
            foreach(BoardItem bi in this.Boards[MASTER].Values.OrderBy(bi => bi.Card.Name).ThenBy(bi => bi.Card.Id))
            {
                string cardId = bi.Card.Id;
                JObject card = new JObject
                {
                    ["Name"] = bi.Card.Name,
                    ["Id"] = cardId
                };

                JArray boards = new JArray();

                foreach(BoardInfo boardInfo in this.BoardInfos)
                {
                    string name = boardInfo.Name;
                    IDictionary<string, BoardItem> boardValue = this.Boards[name];
                    if (boardValue.ContainsKey(cardId))
                    {
                        int normalCount = boardValue[cardId].NormalCount;
                        int foilCount = boardValue[cardId].FoilCount;

                        JObject board = new JObject
                        {
                            ["Name"] = name,
                            ["Normal"] = normalCount,
                            ["Foil"] = foilCount
                        };
                        boards.Add(board);
                    }
                }
                card["Boards"] = boards;
                cards.Add(card);
            }
            result["Cards"] = cards;
            
            string preliminary = result.ToString();

            // Collapse BoardNames to one line by combining lines one at a time
            string prev = null;
            string next = preliminary;
            while(prev != next)
            {
                prev = next;
                next = Regex.Replace(prev, @"(\[[^\]\r?\n]*)\r?\n\s+(""|\])", "$1 $2");
            }
            preliminary = next;

            // Make BoardInfo one line
            preliminary = Regex.Replace(preliminary, @"(\{)\r?\n\s+(""Name"": "".*"",)\r?\n\s+(""Viewable"": (?:true|false),)\r?\n\s+(""Editable"": (?:true|false))\r?\n\s+(\},?)",
                                     "$1 $2 $3 $4 $5");
            // Make a BoardItem one line
            preliminary = Regex.Replace(preliminary, @"(\{)\r?\n\s+(""Name"": "".*"",)\r?\n\s+(""Normal"": [0-9]+,)\r?\n\s+(""Foil"": [0-9]+)\r?\n\s+(\},?)", "$1 $2 $3 $4 $5");

            // Remove newline after open bracket
            preliminary = Regex.Replace(preliminary, @"(\[)\r?\n\s+(\{[^\]\n]*\},?)", "$1 $2");
            // Remove newline before close bracket
            preliminary = Regex.Replace(preliminary, @"(\{[^\]\n]*\},?)\r?\n\s+(\])", "$1 $2");

            // Remove newline after open brace
            preliminary = Regex.Replace(preliminary, @"(\{)\r?\n\s+", "$1 ");
            // Remove newline before close bace
            preliminary = Regex.Replace(preliminary, @"\r?\n\s*(\},?)", " $1");

            // Remove two indents from everything
            preliminary = Regex.Replace(preliminary, @"\n(?:  ){1,2}", "\n");
            // Remove space after colon
            preliminary = Regex.Replace(preliminary, ": ", ":");
            // Line up board definitions
            preliminary = Regex.Replace(preliminary, @"\},\n    \{", "},\n             {");

            return preliminary;
        }

        public string AsDec()
        {
            string result = "";
            foreach(BoardInfo boardInfo in this.BoardInfos)
            {
                string name = boardInfo.Name;
                if (name == MASTER) continue;
                string loc;
                if (name == "Sideboard" || name == "Commander") loc = "SB";
                else loc = "Deck";

                foreach (BoardItem bi in this.Boards[name].Values) {
                    Card card = bi.Card;

                    string line = "///mvid:" + card.MultiverseId + " qty:" + bi.Count.ToString() + " name:" + card.Name + " loc:" + loc;
                    line += "\n";
                    if (loc == "SB") line += "SB: ";
                    line += bi.Count.ToString() + " " + card.Name;
                    if (line.Length != 0) line += "\n";
                    result += line;
                }
            }

            return result;
        }

        public void AddBoard(string name)
        {
            if (Boards.ContainsKey(name))
            {
                return;
            }

            this.Boards[name] = new Dictionary<string, BoardItem>();
            this.BoardInfos.Add(new BoardInfo(name));
            this.changeEventSource.Raise(this, new BoardChangedEventArgs(name, true));
            if(this.AutoSave) this.SaveDeck();
        }

        public void RemoveBoard(string name)
        {
            if (name is null || !Boards.ContainsKey(name) || name == MASTER) return;

            this.Boards.Remove(name);
            for(int i=0; i < this.BoardInfos.Count; i++)
            {
                if(this.BoardInfos[i].Name == name)
                {
                    this.BoardInfos.RemoveAt(i);
                    break;
                }
            }
            this.changeEventSource.Raise(this, new BoardChangedEventArgs(name, false));
            if (this.AutoSave) this.SaveDeck();
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

            if(this.AutoSave) this.SaveDeck();
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

            if(boardName == MASTER)
            {
                foreach(KeyValuePair<string, IDictionary<string, BoardItem>> pair in Boards)
                {
                    if (pair.Key == MASTER) continue;

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

            if(this.AutoSave) this.SaveDeck();
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

        public int GetCountByName(string name, string boardName = MASTER)
        {
            return this.GetCountByNameInternal(name, boardName, bi => bi.Count);
        }

        public int GetNormalCountByName(Card card, string boardName = MASTER)
        {
            return this.GetCountByNameInternal(card.Name, boardName, bi => bi.NormalCount);
        }

        public int GetFoilCountByName(Card card, string boardName = MASTER)
        {
            return this.GetCountByNameInternal(card.Name, boardName, bi => bi.FoilCount);
        }
        
        private int GetCountInBoardInternal(string name, Func<BoardItem, int> toCount)
        {
            if (!this.BoardNames.Contains(name)) return 0;

            int sum = 0;
            foreach(BoardItem boardItem in this.Boards[name].Values)
            {
                sum += toCount(boardItem);
            }
            return sum;
        }

        public int GetCountInBoard(string name)
        {
            return this.GetCountInBoardInternal(name, bi => bi.Count);
        }

        public void BoardInfoRefreshed()
        {
            this.changeEventSource.Raise(this, new BoardChangedEventArgs(null, false));
            if(this.AutoSave) this.SaveDeck();
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

        public class BoardInfo : Model
        {
            private string name = "";
            public string Name { get => name; set => SetProperty(ref name, value); }
            private bool viewable = true; 
            public bool Viewable { get => viewable; set => SetProperty(ref viewable, value); }
            private bool editable = true;
            public bool Editable { get => editable; set => SetProperty(ref editable, value); }
            
            public BoardInfo() { }

            public BoardInfo(string name)
                : this(name, true, true)
            { }

            public BoardInfo(string name, bool viewable, bool editable)
            {
                this.name = name;
                this.viewable = viewable;
                this.editable = editable;
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
