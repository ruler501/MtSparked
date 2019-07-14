using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MtSparked.Interop.FileSystem;
using MtSparked.Interop.Models;
using MtSparked.Core.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// TODO #57: Create IDeckFormat and Implemenation JDecFormat and DecFormat
namespace MtSparked.Core.Decks {
    public static class DeckFormats {

        // TODO #57: Create IDeckFormat and Implemenation JDecFormat and DecFormat
        public static Deck FromJdec(string path) {
            Deck self = new Deck();
            if (ConfigurationManager.FilePicker.PathExists(path)) {
                try {
                    /* TODO #58: Investigate if Decks need a Loading property
                    self.Loading = true;
                    */
                    self.Boards = new Dictionary<string, IDictionary<string, Deck.BoardItem>>() {
                        [Deck.MASTER] = new Dictionary<string, Deck.BoardItem>()
                    };
                    self.BoardInfos = new ObservableCollection<Deck.BoardInfo>();

                    byte[] fileData = ConfigurationManager.FilePicker.OpenFile(path);

                    using (MemoryStream file = new MemoryStream(fileData))
                    using (StreamReader textFile = new StreamReader(file, Encoding.UTF8))
                    using (JsonTextReader reader = new JsonTextReader(textFile)) {
                        JObject deck = (JObject)JToken.ReadFrom(reader);

                        self.Name = (string)deck["Name"];
                        JArray boardInfos = (JArray)deck["Boards"];
                        bool containedMaster = false;
                        foreach (JObject boardInfo in boardInfos) {
                            string name = (string)boardInfo["Name"];
                            bool viewable = (bool)boardInfo["Viewable"];
                            bool editable = (bool)boardInfo["Editable"];
                            self.BoardInfos.Add(new Deck.BoardInfo(name, viewable, editable));
                            self.Boards[name] = new Dictionary<string, Deck.BoardItem>();
                            containedMaster |= name == Deck.MASTER;
                        }
                        if (!containedMaster) {
                            self.BoardInfos.Add(new Deck.BoardInfo(Deck.MASTER));
                        }

                        JArray cards = (JArray)deck["Cards"];
                        foreach (JObject card in cards) {
                            string cardId = (string)card["Id"];

                            Card cardValue = CardDataStore.ById(cardId);

                            JArray boards = (JArray)card["Boards"];
                            // Must process Master first to prevent duplication
                            foreach (JObject board in boards.OrderBy(b => (string)b["Name"] != Deck.MASTER)) {
                                string boardName = (string)board["Name"];
                                int normalCount = (int)board["Normal"];
                                int foilCount = (int)board["Foil"];

                                self.AddCard(cardValue, boardName, true, normalCount);
                                self.AddCard(cardValue, boardName, false, foilCount);
                            }
                        }
                    }
                    self.StoragePath = path;
                } catch (Exception exc) {
                    System.Diagnostics.Debug.Fail(exc.ToString());
                    self = new Deck();
                } finally {
                    /* TODO #58: Investigate if Decks need a Loading property
                    self.Loading = false;
                    */
                }
            }

            return self;
        }

        // TODO #57: Create IDeckFormat and Implemenation JDecFormat and DecFormat
        public static Deck FromDec(string path) {
            Deck self = new Deck {
                Name = Deck.UNNAMED,
                StoragePath = ConfigurationManager.DefaultDeckPath
            };
            if (ConfigurationManager.FilePicker.PathExists(path)) {
                try {
                    // TODO #61: Have a Single Object for Managing Boards and BoardInfos
                    /* TODO #58: Investigate if Decks need a Loading property
                    self.Loading = true;
                    */
                    self.Boards = new Dictionary<string, IDictionary<string, Deck.BoardItem>>() {
                        [Deck.MASTER] = new Dictionary<string, Deck.BoardItem>()
                    };
                    self.BoardInfos = new ObservableCollection<Deck.BoardInfo> { new Deck.BoardInfo(Deck.MASTER) };

                    byte[] fileData = ConfigurationManager.FilePicker.OpenFile(path);

                    string data = Encoding.UTF8.GetString(fileData);

                    string[] lines = data.Split('\n');
                    foreach (string linePreTrim in lines) {
                        string line = linePreTrim.Trim();
                        const string MVID = "mvid:";
                        const string QTY = "qty:";
                        const string LOC = "loc:";
                        int mvidIndex = line.IndexOf(MVID);
                        int qtyIndex = line.IndexOf(QTY);
                        int locIndex = line.IndexOf(LOC);
                        if (mvidIndex < 0 || qtyIndex < 0 || locIndex < 0) {
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
                        if (card is null) {
                            continue;
                        }

                        int count = Int32.Parse(qty);

                        if (loc == Deck.MASTER) {
                            loc = Deck.MASTER + "-1";
                        }

                        self.AddCard(card, loc, true, count);
                    }
                } catch (Exception exc) {
                    System.Diagnostics.Debug.Fail(exc.ToString());
                    self = new Deck();
                } finally {
                    /* TODO #58: Investigate if Decks need a Loading property
                    self.Loading = false;
                    */
                }
            }
            return self;
        }

        // TODO #57: Create IDeckFormat and Implemenation JDecFormat and DecFormat
        public static async void SaveDeckAs(this Deck self) {
            /* TODO #58: Investigate if Decks need a Loading property
            if (self.Loading) {
                return;
            }
            */

            string text = self.ToJDecString();
            byte[] fileContents = Encoding.UTF8.GetBytes(text);

            FileData file = await ConfigurationManager.FilePicker.SaveFileAs(fileContents,
                                                                             $"{self.Name}.jdec");
            if (file is null) {
                // Note failed save
                return;
            }

            if (self.StoragePath != file.FilePath) {
                ConfigurationManager.FilePicker.ReleaseFile(self.StoragePath);
                self.StoragePath = file.FilePath;
            }
        }

        // TODO #57: Create IDeckFormat and Implemenation JDecFormat and DecFormat
        public static async void SaveAsDec(this Deck self) {
            /* TODO #58: Investigate if Decks need a Loading property
            if (self.Loading) {
                return;
            }
            */

            string text = self.AsDec();
            byte[] fileContents = Encoding.UTF8.GetBytes(text);

            FileData file = await ConfigurationManager.FilePicker.SaveFileAs(fileContents, self.Name + ".dec");
            if (file is null) {
                System.Diagnostics.Debug.Fail("Failed to write to file");
                return;
            }
        }

        // TODO #57: Create IDeckFormat and Implemenation JDecFormat and DecFormat
        public static void SaveTempDec(this Deck self) {
            /* TODO #58: Investigate if Decks need a Loading property
            if (self.Loading) {
                return;
            }
            */

            string text = self.AsDec();

            byte[] fileContents = Encoding.UTF8.GetBytes(text);

            ConfigurationManager.FilePicker.SaveFile(fileContents, ConfigurationManager.DefaultTempDecPath);
        }

        // TODO #57: Create IDeckFormat and Implemenation JDecFormat and DecFormat
        public static void SaveDeck(this Deck self) {
            /* TODO #58: Investigate if Decks need a Loading property
            if (self.Loading) {
                return;
            }
            */

            if (self.StoragePath is null) {
                self.StoragePath = ConfigurationManager.DefaultDeckPath;
            }

            string text = self.ToJDecString();

            byte[] fileContents = Encoding.UTF8.GetBytes(text);

            ConfigurationManager.FilePicker.SaveFile(fileContents, self.StoragePath);
        }

        // TODO #57: Create IDeckFormat and Implemenation JDecFormat and DecFormat
        public static string ToJDecString(this Deck self) {
            JObject result = new JObject {
                [Deck.NAME] = self.Name,
            };
            JArray boardInfos = new JArray();
            foreach (Deck.BoardInfo info in self.BoardInfos) {
                boardInfos.Add(new JObject {
                    [Deck.NAME] = info.Name,
                    [Deck.VISIBLE] = info.Visible,
                    [Deck.EDITABLE] = info.Editable
                });
            }
            result[Deck.BOARDS] = boardInfos;

            JArray cards = new JArray();
            foreach (Deck.BoardItem bi in self.Boards[Deck.MASTER].Values.OrderBy(bi => bi.Card.Name)
                                                                         .ThenBy(bi => bi.Card.Id)) {
                // TODO #62: Have Function to Convert BoardItem to JDec Subobject
                string cardId = bi.Card.Id;
                JObject card = new JObject {
                    [Deck.NAME] = bi.Card.Name,
                    [Deck.ID] = cardId
                };

                JArray boards = new JArray();
                // TODO #63: Make it Easier to Lookup What Boards a Card is in and Counts
                foreach (Deck.BoardInfo boardInfo in self.BoardInfos) {
                    string name = boardInfo.Name;
                    IDictionary<string, Deck.BoardItem> boardValue = self.Boards[name];
                    if (boardValue.ContainsKey(cardId)) {
                        int normalCount = boardValue[cardId].NormalCount;
                        int foilCount = boardValue[cardId].FoilCount;

                        JObject board = new JObject {
                            [Deck.NAME] = name,
                            [Deck.NORMAL] = normalCount,
                            [Deck.FOIL] = foilCount
                        };
                        boards.Add(board);
                    }
                }
                card[Deck.BOARDS] = boards;
                cards.Add(card);
            }
            result[Deck.CARDS] = cards;

            if (ConfigurationManager.PrettyPrintJDec) {
                return PrettyPrintJson(result.ToString(Formatting.Indented));
            } else {
                return result.ToString(Formatting.None);
            }
        }

        // TODO #57: Create IDeckFormat and Implemenation JDecFormat and DecFormat
        private static string PrettyPrintJson(string jsonDeck) {
            string preliminary = jsonDeck;
            // Collapse BoardNames to one line by combining lines one at a time
            string prev = null;
            string next = preliminary;
            while (prev != next) {
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

        public static string AsDec(this Deck self) {
            string result = "";
            foreach (Deck.BoardInfo boardInfo in self.BoardInfos) {
                string name = boardInfo.Name;
                if (name == Deck.MASTER) {
                    continue;
                }
                string loc;
                if (name == "Sideboard" || name == "Commander") {
                    loc = "SB";
                } else {
                    loc = "Deck";
                }

                foreach (Deck.BoardItem bi in self.Boards[name].Values) {
                    Card card = bi.Card;

                    string line = $"///mvid:{card.MultiverseId} qty: {bi.Count} name: {card.Name} loc: {loc}";
                    line += "\n";
                    if (loc == "SB") {
                        line += "SB: ";
                    }
                    line += $"{bi.Count} {card.Name}";
                    if (line.Length != 0) {
                        line += "\n";
                    }
                    result += line;
                }
            }

            return result;
        }
    }
}
