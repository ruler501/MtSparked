using Gatherer.Models;
using Newtonsoft.Json.Linq;
using Realms;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Gatherer.Database
{
    class Program
    {
        internal const string BASE_URL = "https://api.scryfall.com";

        static void Main(string[] args)
        {
            IRestClient client = new RestClient(BASE_URL);
            List<Card> cardsArray = new List<Card>();
            JToken json = null;
            string next = BASE_URL + "/cards";
            int totalProcessed = 0;
            do
            {
                DateTime loopStart = DateTime.Now;
                next = next.Substring(BASE_URL.Length);
                Thread.Sleep(50);
                IRestRequest cardsRequest = new RestRequest(next);
                IRestResponse response = client.Execute(cardsRequest);

                json = JToken.Parse(response.Content);

                next = (string)json["next_page"];
                JArray cards = (JArray)json["data"];

                foreach (JToken card in cards)
                {
                    Card value = new Card()
                    {
                        Name = card.Value<string>("name"),
                        Layout = card.Value<string>("layout"),
                        Cmc = card.Value<int?>("cmc"),
                        TypeLine = card.Value<string>("type_line"),
                        Text = card.Value<string>("oracle_text"),
                        ManaCost = card.Value<string>("mana_cost"),
                        Reserved = card.Value<bool>("reserved"),
                        SetCode = card.Value<string>("set"),
                        SetName = card.Value<string>("set_name"),
                        Rarity = card.Value<string>("rarity"),
                        Artist = card.Value<string>("artist"),
                        Border = card.Value<string>("border"),
                        EdhRank = card.Value<int?>("edhrec_rank"),
                        Flavor = card.Value<string>("flavor_text"),
                        Power = card.Value<string>("power"),
                        Toughness = card.Value<string>("toughness"),
                        Life = card.Value<int?>("life_modifier"),
                        Hand = card.Value<int?>("hand_modifier"),
                        IllustrationId = card.Value<string>("illustration_id"),
                        FullArt = card.Value<bool?>("full_art"),
                        Frame = card.Value<string>("frame"),
                        Number = card.Value<string>("collector_number"),
                        Id = card.Value<string>("id"),
                        Watermark = card.Value<string>("watermark")
                    };

                    // Convert to nullable int
                    string loyaltyString = card.Value<string>("loyalty");
                    bool parsed = Int32.TryParse(loyaltyString, out int loyalty);
                    if (parsed)
                    {
                        value.Loyalty = loyalty;
                    }
                    
                    JArray multiverse_ids = card.Value<JArray>("multiverse_ids");
                    if(multiverse_ids is null || multiverse_ids.Count == 0)
                    {
                        value.MultiverseId = value.SetCode + '|' + value.Number;
                    }
                    else if(multiverse_ids.Count == 1)
                    {
                        value.MultiverseId = multiverse_ids[0].Value<int>().ToString();
                    }
                    else
                    {
                        continue;
                        // Something weird is happening likely multifaced cards/splits
                    }

                    JArray colors = card.Value<JArray>("colors");
                    value.Colors = CreateColorList(colors);
                    JArray colorIdentity = card.Value<JArray>("color_identity");
                    value.ColorIdentity = CreateColorList(colorIdentity);
                    JArray colorIndicator = card.Value<JArray>("color_indicator");
                    value.ColorIndicatior = CreateColorList(colorIdentity);

                    JArray faces = card.Value<JArray>("card_faces");
                    if(!(faces is null))
                    {
                        // Deal with multi-faced cards
                    }

                    JToken imageUrls = card.Value<JToken>("image_uris");
                    if(!(imageUrls is null))
                    {
                        value.FullImageUrl = imageUrls.Value<string>("png");
                        value.CroppedImageUrl = imageUrls.Value<string>("art_crop");
                    }

                    JToken legalities = card.Value<JToken>("legalities");
                    if(!(legalities is null))
                    {
                        value.LegalInStandard = legalities.Value<string>("standard") == "legal";
                        value.LegalInFrontier = legalities.Value<string>("frontier") == "legal";
                        value.LegalInModern = legalities.Value<string>("modern") == "legal";
                        value.LegalInPauper = legalities.Value<string>("pauper") == "legal";
                        value.LegalInLegacy = legalities.Value<string>("legacy") == "legal";
                        value.LegalInVintage = legalities.Value<string>("vintage") == "legal";
                        value.LegalInDuelCommander = legalities.Value<string>("duel") == "legal";
                        value.LegalInCommander = legalities.Value<string>("commander") == "legal";
                        value.LegalInMtgoCommander = legalities.Value<string>("1v1") == "legal";
                        value.LegalInFuture = legalities.Value<string>("future") == "legal";
                    }

                    value.Multicolored = value.Colors.Length > 1;
                    value.MulticoloredIdentity = value.ColorIdentity.Length > 1;
                    value.Colorless = value.Colors.Length == 0;
                    value.ColorlessIdentity = value.ColorIdentity.Length == 0;

                    string rulingsUrl = card.Value<string>("rulings_uri");
                    if(!(rulingsUrl is null))
                    {
                        // Really slow
                        CreateRulingsList(rulingsUrl, value.Rulings);
                    }

                    //Need to populate TcgPlayerId will probably be slow

                    // What can we do with this?
                    bool? digital = card.Value<bool?>("digital");

                    cardsArray.Add(value);
                }
                totalProcessed += cards.Count;
                Console.WriteLine((double)100 * totalProcessed / 39021.0);
                // Console.WriteLine(DateTime.Now - loopStart);
            } while ((bool)json["has_more"]);


            RealmConfiguration config = new RealmConfiguration("G:\\Gatherer\\Gatherer\\Gatherer.Database\\cards.db");
            Realm realm = Realm.GetInstance(config);

            Console.WriteLine(cardsArray.Count);
            realm.Write(() =>
           {
               foreach (Card card in cardsArray)
               {
                   realm.Add(card);
               }
           });

            RealmConfiguration config2 = new RealmConfiguration("G:\\Gatherer\\Gatherer\\Gatherer.Database\\cards.compressed.db");
            realm.WriteCopy(config2);
        }

        static string CreateColorList(JArray colors)
        {
            string result = "";
            if (colors is null || colors.Count == 0)
            {
                return "";
            }
            HashSet<string> colorsSet = new HashSet<string>();
            foreach (JToken color in colors)
            {
                string colorValue = color.Value<string>();
                if (!(colorValue is null))
                {
                    colorsSet.Add(colorValue);
                }
            }
            if (colorsSet.Contains("W"))
            {
                result += "W";
            }
            if (colorsSet.Contains("U"))
            {
                result += "U";
            }
            if (colorsSet.Contains("B"))
            {
                result += "B";
            }
            if (colorsSet.Contains("R"))
            {
                result += "R";
            }
            if (colorsSet.Contains("G"))
            {
                result += "G";
            }

            return result;
        }

        static void CreateRulingsList(string url, IList<Ruling> rulingsList)
        {
            RestClient client = new RestClient(BASE_URL);
            string next = url;

            JToken json = null;
            do
            {
                next = next.Substring(BASE_URL.Length);
                Thread.Sleep(50);
                IRestRequest request = new RestRequest(next);
                IRestResponse response = client.Execute(request);

                json = JToken.Parse(response.Content);

                next = (string)json["next_page"];
                JArray rulings = (JArray)json["data"];

                foreach (JToken ruling in rulings)
                {
                    Ruling value = new Ruling()
                    {
                        PublishDate = ruling.Value<DateTime>("published_at"),
                        Comment = ruling.Value<string>("comment")
                    };
                    rulingsList.Add(value);
                }

            } while ((bool)json["has_more"]);
        }
    }
}
