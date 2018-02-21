using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using Gatherer.Models;
using Gatherer.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Gatherer.ViewModels
{
    public class CardsViewModel : INotifyPropertyChanged
    {
        public CardDataStore DataStore;

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private ObservableCollection<Card> items;
        public ObservableCollection<Card> Items { get => items; set => SetProperty(ref items, value); }
        public int CardCount => Items.Count;
        public Command LoadItemsCommand { get; set; }
        
        void ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                this.DataStore.LoadCards();
                
                ObservableCollection<Card> items = new ObservableCollection<Card>(this.DataStore.items);
                this.Items = items;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public CardsViewModel(CardDataStore store)
        {
            Title = "Cards";
            Items = new ObservableCollection<Card>();
            LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
            this.DataStore = store;
            ConfigurationManager.PropertyChanged += this.OnUniqueUpdated;
            this.LoadItemsCommand.Execute(null);
        }

        private void OnUniqueUpdated(object sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName == nameof(ConfigurationManager.ShowUnique))
            {
                this.LoadItemsCommand.Execute(null);
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}