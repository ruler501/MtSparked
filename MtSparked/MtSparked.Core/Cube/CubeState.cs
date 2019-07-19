using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MtSparked.Interop.Models;

namespace MtSparked.Core.Cube.Parser {

    public class PackCard {

        public Card Card { get; set; }
        public bool Foil { get; set; }
        public string BoardName { get; set; }

        public object GetProperty(string propertyName) {
            if (propertyName == "Foil") {
                return this.Foil;
            } else if (propertyName == "Colors") {
                return new ArrayList((this.Card.Colors ?? "").ToList());
            } else if (propertyName == "ColorIdentity") {
                return new ArrayList((this.Card.ColorIdentity ?? "").ToList());
            } else if (propertyName == "ColorIndicator") {
                return new ArrayList((this.Card.ColorIndicator ?? "").ToList());
            } else {
                return typeof(Card).GetProperty(propertyName).GetValue(propertyName);
            }
        }

    }

    public class CubeState {
        private Deck Deck { get; set; }
        public Dictionary<string, List<PackCard>> Boards { get; set; }

        public Dictionary<string, object> VariableValues { get; set; }

        public List<PackCard> Pack { get; set; }

        public CubeState(Deck deck) {
            this.Deck = deck;
            this.Boards = new Dictionary<string, List<PackCard>>();
            foreach (KeyValuePair<string, IDictionary<string, Deck.BoardItem>> pair in deck.Boards) {
                IEnumerable<PackCard> cards = new List<PackCard>();
                foreach (Deck.BoardItem bi in pair.Value.Values) {
                    List<PackCard> foils = Enumerable.Repeat(new PackCard()
                    {
                        Card = bi.Card,
                        Foil = true,
                        BoardName = pair.Key
                    }, bi.FoilCount).ToList();

                    List<PackCard> normals = Enumerable.Repeat(new PackCard()
                    {
                        Card = bi.Card,
                        Foil = true,
                        BoardName = pair.Key
                    }, bi.NormalCount).ToList();
                    cards = cards.Concat(foils).Concat(normals);
                }
                this.Boards.Add(pair.Key, cards.ToList());
            }
            this.VariableValues = new Dictionary<string, object>();
            this.Pack = new List<PackCard>();
        }

        public CubeState(CubeState other) {
            this.Deck = other.Deck;
            this.VariableValues = new Dictionary<string, object>(other.VariableValues);
            this.Pack = other.Pack;
            this.Boards = other.Boards;
        }
    }
}
