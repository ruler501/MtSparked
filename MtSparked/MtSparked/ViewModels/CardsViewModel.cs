using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using MtSparked.Models;
using MtSparked.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MtSparked.ViewModels
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

        private ObservableCollection<EnhancedGrouping<Card>> items;
        public ObservableCollection<EnhancedGrouping<Card>> Items { get => items; set => SetProperty(ref items, value); }
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

                this.Items = new ObservableCollection<EnhancedGrouping<Card>>(this.DataStore.Items);
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
            Items = new ObservableCollection<EnhancedGrouping<Card>>();
            LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
            this.DataStore = store;
            ConfigurationManager.PropertyChanged += this.OnUniqueUpdated;
            this.LoadItemsCommand.Execute(null);
        }

        private void OnUniqueUpdated(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ConfigurationManager.ShowUnique)
                || args.PropertyName == nameof(ConfigurationManager.SortCriteria)
                || args.PropertyName == nameof(ConfigurationManager.DescendingSort)
                || args.PropertyName == nameof(ConfigurationManager.CountByGroup))
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