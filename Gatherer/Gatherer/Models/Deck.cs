using System;
using System.Collections.Generic;
using System.Text;
using WeakEvent;

namespace Gatherer.Models
{
    public class Deck
    {
        public string Name { get; set; }
        public string StoragePath { get; set; }
        public Dictionary<string, DeckCard> Cards;
        public Dictionary<string, ISet<string>> CardsByName { get; set; }

        private readonly WeakEventSource<DeckChangedEventArgs> changeEventSource = new WeakEventSource<DeckChangedEventArgs>();
        public event EventHandler<DeckChangedEventArgs> ChangeEvent
        {
            add { changeEventSource.Subscribe(value); }
            remove { changeEventSource.Unsubscribe(value); }
        }

        public Deck()
        {
            this.Cards = new Dictionary<string, DeckCard>();
            this.CardsByName = new Dictionary<string, ISet<string>>();
        }

        public void AddCard(Card card, bool normal = true, int amount=1)
        {
            DeckCard cardItem;
            if (Cards.ContainsKey(card.Id))
            {
                cardItem = Cards[card.Id];
            }
            else
            {
                cardItem = new DeckCard()
                {
                    Card = card
                };
                Cards[card.Id] = cardItem;
                if (CardsByName.ContainsKey(card.Name))
                {
                    CardsByName[card.Name].Add(card.Id);
                }
                else
                {
                    CardsByName.Add(card.Name, new HashSet<string> { card.Id });
                }
            }

            if (normal) cardItem.NormalCount += amount;
            else cardItem.FoilCount += amount;

            changeEventSource.Raise(this, new DeckChangedEventArgs(card, normal, amount));
        }

        public void RemoveCard(Card card, bool normal = true, int amount = 1)
        {

            DeckCard cardItem;
            if (Cards.ContainsKey(card.Id))
            {
                cardItem = Cards[card.Id];
            }
            else
            {
                throw new KeyNotFoundException();
            }

            if (normal) cardItem.NormalCount -= amount;
            else cardItem.FoilCount -= amount;

            if(cardItem.NormalCount < 0) cardItem.NormalCount = 0;
            if (cardItem.FoilCount < 0) cardItem.FoilCount = 0;

            if(cardItem.NormalCount == 0 && cardItem.FoilCount == 0)
            {
                Cards.Remove(card.Id);
                CardsByName[card.Name].Remove(card.Id);
                if(CardsByName[card.Name].Count == 0)
                {
                    CardsByName.Remove(card.Name);
                }
            }

            changeEventSource.Raise(this, new DeckChangedEventArgs(card, normal, amount));
        }

        public int GetNormalCount(Card card)
        {
            if (!this.Cards.ContainsKey(card.Id))
            {
                return 0;
            }
            return this.Cards[card.Id].NormalCount;
        }

        public int GetFoilCount(Card card)
        {
            if (!this.Cards.ContainsKey(card.Id))
            {
                return 0;
            }
            return this.Cards[card.Id].FoilCount;
        }

        public class DeckCard
        {
            public Card Card { get; set; }
            public int FoilCount { get; set; }
            public int NormalCount { get; set; }

            // To be used in the future
            public ISet<string> Boards { get; set; }

            public int Count => NormalCount + FoilCount;
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
