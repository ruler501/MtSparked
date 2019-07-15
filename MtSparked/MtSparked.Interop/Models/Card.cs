using System;
using System.Collections.Generic;
using MtSparked.Interop.Databases;

namespace MtSparked.Interop.Models
{
    // Realm implements the relevant parts of Model for us.
    public class Card : ModelWithId
    {
        public const int SchemaVersion = 1;

        private string oracle;
        public string Oracle {
            get { return this.oracle; }
            set { _ = this.SetProperty(ref this.oracle, value); }
        }

        public string ManaCost { get; set; }
        public string Name { get; set; }
        public string Rarity { get; set; }
        public string Watermark { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public string Flavor { get; set; }
        public string Artist { get; set; }
        public string Border { get; set; }
        public string Frame { get; set; }
        public string Layout { get; set; }
        public string TypeLine { get; set; }
        public string Colors { get; set; }
        public string ColorIdentity { get; set; }
        public int Cmc { get; set; }
        public string ColorIndicator { get; set; }
        public bool Multicolored { get; set; }
        public bool Colorless { get; set; }
        public bool ReservedList { get; set; }
        public bool Reprint { get; set; }
        public bool FullArt { get; set; }
        public bool OnlineOnly { get; set; }
        public bool LegalInStandard { get; set; }
        public bool LegalInFrontier { get; set; }
        public bool LegalInModern { get; set; }
        public bool LegalInPauper { get; set; }
        public bool LegalInLegacy { get; set; }
        public bool LegalInPennyDreadful { get; set; }
        public bool LegalInDuelCommander { get; set; }
        public bool LegalInCommander { get; set; }
        public bool LegalInMtgoCommander { get; set; }
        public bool LegalInNextStandard { get; set; }

        // None of the below are able to be searched with yet
        public string Power { get; set; }
        public string Toughness { get; set; }

        public double? MarketPrice { get; set; }
        public int? EdhRank { get; set; }
        public int? Hand { get; set; }
        public int? Life { get; set; }
        public int? Loyalty { get; set; }

        private string multiverseId;
        public string MultiverseId {
            get { return this.multiverseId; }
            set { _ = this.SetProperty(ref this.multiverseId, value); }
        }
        public string IllustrationId { get; set; }

        public string Number { get; set; }
        public string FullImageUrl { get; set; }
        public string CroppedImageUrl { get; set; }
        public string TcgPlayerId { get; set; }
        public IList<Ruling> Rulings { get; }

        // Ignored due to annoyances with restricted vs banned vs not legal
        // [Indexed]
        // public bool LegalInVintage { get; set; }

        public class Ruling : Model {

            public DateTimeOffset PublishDate { get; set; }
            public string Comment { get; set; }

        }

    }
}
