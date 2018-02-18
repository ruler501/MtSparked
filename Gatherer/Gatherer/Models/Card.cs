using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gatherer.Models
{
    public class Card : RealmObject
    {
        [Indexed]
        public string Text { get; set; }
        [Indexed]
        public string ManaCost { get; set; }
        [Indexed]
        public string Power { get; set; }
        [Indexed]
        public string Toughness { get; set; }
        [Indexed]
        public bool Reserved { get; set; }
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
        public bool Reprint { get; set; }
        [Indexed]
        public string Border { get; set; }
        [Indexed]
        public string Frame { get; set; }
        [Indexed]
        public string Layout { get; set; }
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
        public bool LegalInVintage { get; set; }
        [Indexed]
        public bool LegalInDuelCommander { get; set; }
        [Indexed]
        public bool LegalInCommander { get; set; }
        [Indexed]
        public bool LegalInMtgoCommander { get; set; }
        [Indexed]
        public bool LegalInFuture { get; set; }
        [Indexed]
        public string MultiverseId { get; set; }
        [PrimaryKey]
        public string Id { get; set; }
        public int? EdhRank { get; set; }
        public string IllustrationId { get; set; }
        public string Number { get; set; }
        public string FullImageUrl { get; set; }
        public string CroppedImageUrl { get; set; }
        public bool? FullArt { get; set; }
        public string TypeLine { get; set; }
        public int? Cmc { get; set; }
        public int? Hand { get; set; }
        public int? Life { get; set; }
        public int? Loyalty { get; set; }
        public string TcgPlayerId { get; set; }
        public bool Multicolored { get; set; }
        public bool MulticoloredIdentity { get; set; }
        public IList<char> Colors { get; }
        public IList<char> ColorIdentity { get; }
        public IList<char> ColorIndicator { get; }
        public IList<Ruling> Rulings { get; }
    }

    public class Ruling : RealmObject
    {
        public DateTimeOffset PublishDate { get; set; }
        public string Comment { get; set; }
    }
}
