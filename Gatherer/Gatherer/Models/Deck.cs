﻿using System;
using System.Collections.Generic;
using System.Text;
using WeakEvent;

namespace Gatherer.Models
{
    public class Deck
    {
        public const string MASTER = "Master";

        public string Name { get; set; }
        public string StoragePath { get; set; }
        public IDictionary<string, IDictionary<string, BoardItem>> Boards;
        public IDictionary<string, ISet<string>> CardsByName { get; set; }

        public ICollection<string> BoardNames => this.Boards.Keys;
        public IDictionary<string, BoardItem> Master => this.Boards[MASTER];
        public ICollection<BoardItem> Cards => this.Master.Values;
        public ICollection<string> CardNames => this.CardsByName.Keys;


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

        public void AddBoard(string name)
        {
            if (Boards.ContainsKey(name))
            {
                return;
            }

            this.Boards[name] = new Dictionary<string, BoardItem>();
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

            changeEventSource.Raise(this, new DeckChangedEventArgs(card, normal, amount));
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

            changeEventSource.Raise(this, new DeckChangedEventArgs(card, normal, amount));
        }

        public int GetNormalCount(Card card, string boardName=MASTER)
        {
            if (!this.Boards.ContainsKey(boardName) || !this.Boards[boardName].ContainsKey(card.Id))
            {
                return 0;
            }
            return this.Boards[boardName][card.Id].NormalCount;
        }

        public int GetFoilCount(Card card, string boardName=MASTER)
        {
            if (!this.Boards.ContainsKey(boardName) || !this.Boards[boardName].ContainsKey(card.Id))
            {
                return 0;
            }
            return this.Boards[boardName][card.Id].FoilCount;
        }

        public int GetCount(Card card, string boardName = MASTER)
        {
            if (!this.Boards.ContainsKey(boardName) || !this.Boards[boardName].ContainsKey(card.Id))
            {
                return 0;
            }
            return this.Boards[boardName][card.Id].Count;
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
        private Card card;
        private int amount;
        private bool normal;

        public DeckChangedEventArgs(Card card, bool normal, int amount)
        {
            this.card = card;
            this.amount = amount;
            this.normal = normal;
        }

        public Card Card => card;
        public int Amount => amount;
        public bool Normal => Normal;
    }
}
