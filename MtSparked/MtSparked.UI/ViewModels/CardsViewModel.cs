using System;
using System.ComponentModel;

using Xamarin.Forms;

using MtSparked.Interop.Models;
using MtSparked.Core.Services;
using System.Collections.ObjectModel;

namespace MtSparked.UI.ViewModels {
    public class CardsViewModel : Model {
        
        public CardDataStore DataStore { get; }

        private bool isBusy = false;
        public bool IsBusy {
            get { return this.isBusy; }
            set { _ = this.SetProperty(ref this.isBusy, value); }
        }

        private string title = "Cards";
        public string Title {
            get { return this.title; }
            set { _ = this.SetProperty(ref this.title, value); }
        }

        private ObservableCollection<EnhancedGrouping<Card>> items = new ObservableCollection<EnhancedGrouping<Card>>();
        public ObservableCollection<EnhancedGrouping<Card>> Items {
            get { return this.items; }
            set { _ = this.SetProperty(ref this.items, value); }
        }
        public int CardCount => this.Items.Count;
        public Command LoadItemsCommand { get; private set; }

        private void ExecuteLoadItemsCommand() {
            if (this.IsBusy) {
                return;
            }

            this.IsBusy = true;

            try {
                this.DataStore.LoadCards();

                this.Items = new ObservableCollection<EnhancedGrouping<Card>>(this.DataStore.Items);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex);
            } finally {
                this.IsBusy = false;
            }
        }

        public CardsViewModel(CardDataStore store) {
            this.LoadItemsCommand = new Command(() => this.ExecuteLoadItemsCommand());
            this.DataStore = store;
            ConfigurationManager.PropertyChanged += this.OnUniqueUpdated;
            this.LoadItemsCommand.Execute(null);
        }

        private void OnUniqueUpdated(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == nameof(ConfigurationManager.ShowUnique)
                || args.PropertyName == nameof(ConfigurationManager.SortCriteria)
                || args.PropertyName == nameof(ConfigurationManager.DescendingSort)
                || args.PropertyName == nameof(ConfigurationManager.CountByGroup)) {
                this.LoadItemsCommand.Execute(null);
            }
        }

    }
}