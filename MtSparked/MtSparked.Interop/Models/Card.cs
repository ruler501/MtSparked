using Realms;
using System;
using System.Collections.Generic;

namespace MtSparked.Interop.Models
{
    // Realm implements the relevant parts of Model for us.
    public class Card : RealmObject
    {
        public const int SchemaVersion = 1;

        [Indexed]
        public string Text { get; set; }
        [Indexed]
        public string ManaCost { get; set; }
        [Indexed]
        public string Name { get; set; }
        [Indexed]
        public string Rarity { get; set; }
        [Indexed]
        public string Watermark { get; set; }
        [Indexed]
        public string SetCode { get; set; }
        [Indexed]
        public string SetName { get; set; }
        [Indexed]
        public string Flavor { get; set; }
        [Indexed]
        public string Artist { get; set; }
        [Indexed]
        public string Border { get; set; }
        [Indexed]
        public string Frame { get; set; }
        [Indexed]
        public string Layout { get; set; }
        [Indexed]
        public string TypeLine { get; set; }
        [Indexed]
        public string Colors { get; set; }
        [Indexed]
        public string ColorIdentity { get; set; }
        [Indexed]
        public int Cmc { get; set; }
        [Indexed]
        public string ColorIndicator { get; set; }
        [Indexed]
        public bool Multicolored { get; set; }
        [Indexed]
        public bool Colorless { get; set; }
        [Indexed]
        public bool ReservedList { get; set; }
        [Indexed]
        public bool Reprint { get; set; }
        [Indexed]
        public bool FullArt { get; set; }
        [Indexed]
        public bool OnlineOnly { get; set; }
        [Indexed]
        public bool LegalInStandard { get; set; }
        [Indexed]
        public bool LegalInFrontier { get; set; }
        [Indexed]
        public bool LegalInModern { get; set; }
        [Indexed]
        public bool LegalInPauper { get; set; }
        [Indexed]
        public bool LegalInLegacy { get; set; }
        [Indexed]
        public bool LegalInPennyDreadful { get; set; }
        [Indexed]
        public bool LegalInDuelCommander { get; set; }
        [Indexed]
        public bool LegalInCommander { get; set; }
        [Indexed]
        public bool LegalInMtgoCommander { get; set; }
        [Indexed]
        public bool LegalInNextStandard { get; set; }

        // None of the below are able to be searched with yet
        [Indexed]
        public string Power { get; set; }
        [Indexed]
        public string Toughness { get; set; }

        // Searchable but not indexed
        public double? MarketPrice { get; set; }
        public int? EdhRank { get; set; }
        public int? Hand { get; set; }
        public int? Life { get; set; }
        public int? Loyalty { get; set; }

        [PrimaryKey]
        public string Id { get; set; }
        [Indexed]
        public string MultiverseId { get; set; }
        [Indexed]
        public string IllustrationId { get; set; }

        public string Number { get; set; }
        public string FullImageUrl { get; set; }
        public string CroppedImageUrl { get; set; }
        public string TcgPlayerId { get; set; }
        public IList<Ruling> Rulings { get; }

        // Ignored due to annoyances with restricted vs banned vs not legal
        // [Indexed]
        // public bool LegalInVintage { get; set; }

        public class Ruling : RealmObject {
            public DateTimeOffset PublishDate { get; set; }
            public string Comment { get; set; }

        }

        public static void MigrationCallback(Migration migration, ulong oldSchemaVersion) {
            // None needed yet.
        }
    }

    public class CardEqualityComparer : EqualityComparer<Card> {

        public override bool Equals(Card a, Card b) => a.Id == b.Id;

        public override int GetHashCode(Card c) => c.Id.GetHashCode();

    }
}
