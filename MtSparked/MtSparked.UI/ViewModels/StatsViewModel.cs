using MtSparked.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Microcharts.Forms;
using Microcharts;

namespace MtSparked.ViewModels
{
    public class StatsViewModel : Model
    {
        private ObservableCollection<string> boardNames = new ObservableCollection<string>();
        public ObservableCollection<string> BoardNames
        {
            get => boardNames;
            set => SetProperty(ref boardNames, value);
        }

        private string boardName = Deck.MASTER;
        public string BoardName
        {
            get => boardName;
            set
            {
                if (boardName != value)
                {
                    SetProperty(ref boardName, value);
                    this.RefreshStats();
                }
            }
        }

        private string manaCounts = "";
        public string ManaCounts
        {
            get => manaCounts;
            set => SetProperty(ref manaCounts, value);
        }

        private Chart cmcChart = new LineChart();
        public Chart CmcChart
        {
            get => cmcChart;
            set => SetProperty(ref cmcChart, value);
        }

        private Deck Deck { get; set; }

        public StatsViewModel(Deck deck)
        {
            this.Deck = deck;

            this.OnDeckUpdated(null, new BoardChangedEventArgs(null, false));
            this.RefreshStats();

            this.Deck.ChangeEvent += this.OnDeckUpdated;
        }

        public void OnDeckUpdated(object sender, DeckChangedEventArgs args)
        {
            if (args is BoardChangedEventArgs)
            {
                this.BoardNames = new ObservableCollection<string>(this.Deck.BoardInfos.Where(bi => bi.Viewable).Select(bi => bi.Name));

                if (!(this.BoardName is null))
                {
                    int index = this.boardNames.IndexOf(this.boardName);
                    if (index >= 0)
                    {
                        Device.BeginInvokeOnMainThread(() => this.BoardName = this.BoardNames[index]);
                        return;
                    }
                }

                if (this.BoardNames.Count > 0)
                {
                    Device.BeginInvokeOnMainThread(() => this.BoardName = this.BoardNames[0]);
                }
            }
            else if(args is CardCountChangedEventArgs)
            {
                this.RefreshStats();
            }
        }

        public void RefreshStats()
        {
            if (this.Deck is null || this.BoardName is null || !this.Deck.Boards.ContainsKey(this.BoardName))
            {
                return;
            }
            string[] ManaTypes = new[]
            {
                "W", "U", "B", "R", "G"
            };
            Dictionary<string, int> manaCosts = new Dictionary<string, int>(ManaTypes.Length);
            foreach(string mana in ManaTypes)
            {
                manaCosts[mana] = 0;
            }
            Dictionary<int, int> cmcCounts = new Dictionary<int, int>();

            foreach(Deck.BoardItem item in this.Deck.Boards[this.BoardName].Values
                .Where(bi => !bi.Card.TypeLine.Contains("Land")))
            {
                int count = item.Count;
                int cmc = item.Card.Cmc;
                string manaCost = item.Card.ManaCost;

                if (cmcCounts.ContainsKey(cmc))
                {
                    cmcCounts[cmc] += count;
                }
                else
                {
                    cmcCounts[cmc] = count;
                }

                string[] manaPieces = manaCost.Replace("{", "").Split('}');
                foreach(string piece in manaPieces)
                {
                    if(manaCosts.ContainsKey(piece)) manaCosts[piece] += count;
                }
            }
            
            if (cmcCounts.Count > 0)
            {
                int min = cmcCounts.Keys.Min();
                int max = cmcCounts.Keys.Max() + 1;

                for (int i = min; i <= max; i++)
                {
                    if (!cmcCounts.ContainsKey(i)) cmcCounts[i] = 0;
                }
            }
            else
            {
                cmcCounts[0] = 0;
            }

            Chart cmcChart = new LineChart()
            {
                Entries = cmcCounts.OrderBy(p => p.Key).Select(p => new Microcharts.Entry(p.Value)
                {
                    Label = p.Key.ToString(),
                    ValueLabel = p.Value.ToString()
                }),
                MinValue = 0,
                MaxValue = Math.Max(cmcCounts.Values.Max(), 1),
                LineMode = LineMode.Straight
            };
            this.CmcChart = cmcChart;

            double total = manaCosts.Select(p => p.Value).Sum();
            string manaCount = "Total: " + ((int)total).ToString();
            foreach (string mana in ManaTypes)
            {
                if(manaCosts[mana] > 0)
                {
                    manaCount += "\n" + mana + ": " + manaCosts[mana].ToString()
                                 + " / " + (100* manaCosts[mana]/total).ToString("0.00") + "%";
                }
            }
            this.ManaCounts = manaCount;
        }
    }
}
