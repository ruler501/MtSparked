using System;
using System.Collections.Generic;
using System.Text;

namespace Gatherer.Models
{
    public class Deck
    {
        public string Name { get; set; }
        public string StoragePath { get; set; }
        public Dictionary<string, DeckCard> Cards;
        public Dictionary<string, ISet<string>> CardsByName { get; set; }

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

            if(cardItem.NormalCount < 0 || cardItem.FoilCount < 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Tried to remove more than there were items");
            }

            if(cardItem.NormalCount == 0 && cardItem.FoilCount == 0)
            {
                Cards.Remove(card.Id);
                CardsByName[card.Name].Remove(card.Id);
                if(CardsByName[card.Name].Count == 0)
                {
                    CardsByName.Remove(card.Name);
                }
            }
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
}
