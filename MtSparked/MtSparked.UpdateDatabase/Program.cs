using MtSparked.Interop.Models;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MtSparked.UpdateDatabase {
    public class Program {
        public const string PLACEHOLDER_FULL_IMAGE_URL = null;
        public const string PLACEHOLDER_CROPPED_IMAGE_URL = null;
        public const string REALM_DB_PATH = "G:\\Gatherer\\MtSparked\\MtSparked.Database\\cards.db";
        public const string REALM_COMPRESSED_DB_PATH = "G:\\Gatherer\\MtSparked\\MtSparked.Database\\cards.compressed.db";

        internal const string BASE_URL = "https://api.scryfall.com";

#pragma warning disable IDE0060 // Remove unused parameter
        public static void Main(string[] _IgnoredParam) {
#pragma warning restore IDE0060 // Remove unused parameter
            IRestClient client = new RestClient(BASE_URL);
            List<Card> cardsArray = new List<Card>();
            string next = BASE_URL + "/cards";
            int totalProcessed = 0;
            JToken json;
            do {
                next = next.Substring(BASE_URL.Length);
                Thread.Sleep(50);
                IRestRequest cardsRequest = new RestRequest(next);
                IRestResponse response = client.Execute(cardsRequest);

                json = JToken.Parse(response.Content);

                next = json.Value<string>("next_page");
                JArray cards = json.Value<JArray>("data");

                foreach (JToken card in cards) {
                    Card value = new Card() {
                        Name = card.Value<string>("name"),
                        Layout = card.Value<string>("layout"),
                        Cmc = card.Value<int?>("cmc") ?? 0,
                        TypeLine = card.Value<string>("type_line"),
                        Oracle = card.Value<string>("oracle_text"),
                        ManaCost = card.Value<string>("mana_cost"),
                        ReservedList = card.Value<bool?>("reserved") ?? false,
                        Reprint = card.Value<bool?>("reprint") ?? false,
                        SetCode = card.Value<string>("set")?.ToUpper(),
                        SetName = card.Value<string>("set_name"),
                        Rarity = card.Value<string>("rarity"),
                        Artist = card.Value<string>("artist"),
                        Border = card.Value<string>("border_color"),
                        EdhRank = card.Value<int?>("edhrec_rank"),
                        Flavor = card.Value<string>("flavor_text"),
                        Power = card.Value<string>("power"),
                        Toughness = card.Value<string>("toughness"),
                        Life = card.Value<int?>("life_modifier"),
                        Hand = card.Value<int?>("hand_modifier"),
                        IllustrationId = card.Value<string>("illustration_id"),
                        FullArt = card.Value<bool?>("full_art") ?? false,
                        Frame = card.Value<string>("frame"),
                        Number = card.Value<string>("collector_number"),
                        Id = card.Value<string>("id"),
                        Watermark = card.Value<string>("watermark"),
                        OnlineOnly = card.Value<bool?>("digital") ?? false,
                        MarketPrice = card.Value<double?>("usd")
                    };

                    string loyaltyString = card.Value<string>("loyalty");
                    if (Int32.TryParse(loyaltyString, out int loyalty)) {
                        value.Loyalty = loyalty;
                    } else {
                        value.Loyalty = null;
                    }

                    JArray multiverse_ids = card.Value<JArray>("multiverse_ids");
                    if (multiverse_ids is null || multiverse_ids.Count == 0) {
                        value.MultiverseId = value.SetCode + '|' + value.Number;
                    } else if (multiverse_ids.Count == 1) {
                        value.MultiverseId = multiverse_ids[0].Value<int>().ToString();
                    } else {
                        // TODO #92: Correctly Handle Loading MultiFaced and Split Cards Into Card Models
                        System.Diagnostics.Debug.WriteLine($"Multiple MultiverseIds: {card.Value<string>("id")}, {card.Value<string>("name")}");
                        continue;
                    }

                    JArray colors = card.Value<JArray>("colors");
                    value.Colors = CreateColorList(colors);
                    JArray colorIdentity = card.Value<JArray>("color_identity");
                    value.ColorIdentity = CreateColorList(colorIdentity);
                    JArray colorIndicator = card.Value<JArray>("color_indicator");
                    value.ColorIndicator = CreateColorList(colorIdentity);

                    JArray faces = card.Value<JArray>("card_faces");
                    if (!(faces is null)) {
                        System.Diagnostics.Debug.WriteLine($"Multiple Faces: {card.Value<string>("id")}, {card.Value<string>("name")}");

                        // TODO #92: Correctly Handle Loading MultiFaced and Split Cards Into Card Models
                        continue;
                    }

                    JToken imageUrls = card.Value<JToken>("image_uris");
                    if (!(imageUrls is null)) {
                        value.FullImageUrl = imageUrls.Value<string>("png");
                        value.CroppedImageUrl = imageUrls.Value<string>("art_crop");
                    } else {
                        value.FullImageUrl = PLACEHOLDER_FULL_IMAGE_URL;
                        value.CroppedImageUrl = PLACEHOLDER_CROPPED_IMAGE_URL;
                    }

                    JToken legalities = card.Value<JToken>("legalities");
                    if (!(legalities is null)) {
                        value.LegalInStandard = legalities.Value<string>("standard") == "legal";
                        value.LegalInFrontier = legalities.Value<string>("frontier") == "legal";
                        value.LegalInModern = legalities.Value<string>("modern") == "legal";
                        value.LegalInPauper = legalities.Value<string>("pauper") == "legal";
                        value.LegalInLegacy = legalities.Value<string>("legacy") == "legal";
                        // TODO #93: Redo Legality to Handle Vintage
                        // value.LegalInVintage = legalities.Value<string>("vintage") == "legal";
                        value.LegalInDuelCommander = legalities.Value<string>("duel") == "legal";
                        value.LegalInCommander = legalities.Value<string>("commander") == "legal";
                        value.LegalInMtgoCommander = legalities.Value<string>("1v1") == "legal";
                        value.LegalInNextStandard = legalities.Value<string>("future") == "legal";
                        value.LegalInPennyDreadful = legalities.Value<string>("penny") == "legal";
                    }

                    value.Multicolored = value.Colors.Length > 1;
                    value.Colorless = value.Colors.Length == 0;

                    string rulingsUrl = card.Value<string>("rulings_uri");
                    // TODO #94: Find a Feasible Way to Download Rulings
                    if (!(rulingsUrl is null)) {
                        // Really slow
                        // CreateRulingsList(rulingsUrl, value.Rulings);
                    }

                    // TODO #95: Find a Feasible Way to Populate TcgPlayerId
                    // We can leave the pricing in the model as the data from Scryfall but we need the TcgPlayerId
                    // so we can refresh and update that information dynamically.

                    cardsArray.Add(value);
                }
                totalProcessed += cards.Count;
                Console.WriteLine((double)100 * totalProcessed / json.Value<int>("total_cards"));
            } while (json.Value<bool>("has_more"));
        }

        private static string CreateColorList(JArray colors) {
            string result = "";
            if (colors is null || colors.Count == 0) {
                return "";
            }
            HashSet<string> colorsSet = new HashSet<string>();
            foreach (JToken color in colors) {
                string colorValue = color.Value<string>();
                if (!(colorValue is null)) {
                    _ = colorsSet.Add(colorValue);
                }
            }
            // TODO #65: Custom Class for Dealing with Color
            foreach (char colorChar in "WUBRG") {
                string color = new string(colorChar, 1);
                if(colorsSet.Contains(color)) {
                    result += color;
                }
            }
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0040:Add accessibility modifiers", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        static void CreateRulingsList(string url, IList<Card.Ruling> rulingsList) {
            RestClient client = new RestClient(BASE_URL);
            string next = url;
            int count = 0;

            JToken json;
            do {
                next = next.Substring(BASE_URL.Length);
                Thread.Sleep(50);
                IRestRequest request = new RestRequest(next);
                IRestResponse response = client.Execute(request);

                json = JToken.Parse(response.Content);

                next = json.Value<string>("next_page");
                JArray rulings = json.Value<JArray>("data");

                foreach (JToken ruling in rulings) {
                    Card.Ruling value = new Card.Ruling() {
                        PublishDate = ruling.Value<DateTime>("published_at"),
                        Comment = ruling.Value<string>("comment")
                    };
                    rulingsList.Add(value);
                }
                count += rulings.Count;
            } while (json.Value<bool>("has_more"));
        }

    }
}
