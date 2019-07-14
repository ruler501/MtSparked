using System;
using System.Linq;
using MathNet.Numerics.Distributions;
// using Microcharts;
using MtSparked.Interop.Models;
using MtSparked.UI.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.UI.Views.Decks {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StatsPage : ContentPage {

        private int PopulationSize { get; set; }
        private int SuccessesCount { get; set; }
        private int SamplesCount { get; set; }

        public StatsPage (Deck deck) {
            this.BindingContext = new StatsViewModel(deck);
            this.PopulationSize = this.SuccessesCount = this.SamplesCount = 0;

            this.InitializeComponent();

            this.CalculateHypergeometric();
        }

        public void CalculateHypergeometric() {
            if (this.SamplesCount > this.PopulationSize || this.SuccessesCount > this.PopulationSize) {
                return;
            }
            Hypergeometric dist = new Hypergeometric(this.PopulationSize, this.SuccessesCount, this.SamplesCount);
            /*
            LineChart cdfchart = new LineChart() {
                Entries = Enumerable.Range(0, this.SamplesCount + 1).Select(i =>
                    new Microcharts.Entry(1 - (float)dist.CumulativeDistribution(i-1)) {
                        Label = i.ToString(),
                        ValueLabel = (100 - dist.CumulativeDistribution(i-1) * 100).ToString("0.00") + "%"
                    }),
                MinValue = 0,
                MaxValue = 1,
                LineMode = LineMode.Straight
            };
            this.HyperGeometricCdfChart.Chart = cdfchart;

            LineChart pdfchart = new LineChart() {
                Entries = Enumerable.Range(0, this.SamplesCount + 1).Select(i =>
                    new Microcharts.Entry((float)dist.Probability(i)) {
                        Label = i.ToString(),
                        ValueLabel = (dist.Probability(i) * 100).ToString("0.00") + "%"
                    }),
                MinValue = 0,
                MaxValue = 1,
                LineMode = LineMode.Straight
            };
            this.HyperGeometricPdfChart.Chart = pdfchart;
            */
        }

        public void OnPopulationChanged(object sender, TextChangedEventArgs args) {
            const string DEFAULT_TEXT = "Deck Size";
            if (!String.IsNullOrWhiteSpace(args.NewTextValue) && args.NewTextValue != DEFAULT_TEXT) {
                bool valid = Int32.TryParse(args.NewTextValue, out int population);

                if (valid) {
                    if (this.PopulationSize != population) {
                        this.PopulationSize = population;
                        this.CalculateHypergeometric();
                    }
                } else {
                    if (Int32.TryParse(args.OldTextValue, out _)) {
                        this.PopulationEntry.Text = args.OldTextValue;
                    } else {
                        this.PopulationEntry.Text = "";
                    }
                }
            }
        }

        public void OnSuccessesChanged(object sender, TextChangedEventArgs args) {
            const string DEFAULT_TEXT = "Successes";
            if (!String.IsNullOrWhiteSpace(args.NewTextValue) && args.NewTextValue != DEFAULT_TEXT) {
                bool valid = Int32.TryParse(args.NewTextValue, out int successes);

                if (valid) {
                    if (this.SuccessesCount != successes) {
                        this.SuccessesCount = successes;
                        this.CalculateHypergeometric();
                    }
                } else {
                    if (Int32.TryParse(args.OldTextValue, out _)) {
                        this.SuccessesEntry.Text = args.OldTextValue;
                    } else {
                        this.SuccessesEntry.Text = "";
                    }
                }
            }
        }

        public void OnSamplesChanged(object sender, TextChangedEventArgs args) {
            const string DEFAULT_TEXT = "Successes";
            if (!String.IsNullOrWhiteSpace(args.NewTextValue) && args.NewTextValue != DEFAULT_TEXT) {
                bool valid = Int32.TryParse(args.NewTextValue, out int samples);

                if (valid) {
                    if (this.SamplesCount != samples) {
                        this.SamplesCount = samples;
                        this.CalculateHypergeometric();
                    }
                } else {
                    valid = Int32.TryParse(args.OldTextValue, out samples);
                    if (valid) {
                        this.SamplesEntry.Text = args.OldTextValue;
                    } else {
                        this.SamplesEntry.Text = "";
                    }
                }
            }
        }

    }
}