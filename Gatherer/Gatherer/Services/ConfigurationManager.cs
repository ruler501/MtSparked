using Gatherer.FilePicker;
using Gatherer.Models;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using WeakEvent;
using Xamarin.Forms;

namespace Gatherer.Services
{
    public static class ConfigurationManager
    {
        private const string ACTIVE_DECK_KEY = "ActiveDeck";
        public static string DefaultDeckPath => Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "temp.jdec");
        public static string DefaultTempDecPath => Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "temp.dec");
        private static Deck activeDeck = null;
        public static Deck ActiveDeck {
            get
            {
                if(activeDeck is null)
                {
                    string activeDeckPath = AppSettings.GetValueOrDefault(ACTIVE_DECK_KEY, null);
                    if(activeDeckPath is null || !FilePicker.PathExists(activeDeckPath))
                    {
                        ActiveDeck = Deck.FromJdec(DefaultDeckPath);
                    }
                    else
                    {
                        ActiveDeck = Deck.FromJdec(activeDeckPath);
                    }
                }
                return activeDeck;
            }
            set
            {
                string path = null;
                if (!(activeDeck is null))
                {
                    activeDeck.ChangeEvent -= ConfigurationManager.UpdateDeckPath;
                    path = activeDeck.StoragePath;
                }
                activeDeck = value;
                if(activeDeck is null)
                {
                    activeDeck = Deck.FromJdec(DefaultDeckPath);
                }

                if (FilePicker.PathExists(activeDeck.StoragePath))
                {
                    AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, activeDeck.StoragePath);
                }
                else
                {
                    activeDeck.StoragePath = DefaultDeckPath;
                    AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, activeDeck.StoragePath);
                }
                if (!(activeDeck is null))
                {
                    activeDeck.ChangeEvent += ConfigurationManager.UpdateDeckPath;
                    OnPropertyChanged();
                }
            }
        }

        private static string SHOW_UNIQUE_KEY = "ShowUnique";
        public static bool ShowUnique
        {
            get => AppSettings.GetValueOrDefault(SHOW_UNIQUE_KEY, false);
            set
            {
                AppSettings.AddOrUpdateValue(SHOW_UNIQUE_KEY, value);
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged
        private static WeakEventSource<PropertyChangedEventArgs> propertyChangedSource = new WeakEventSource<PropertyChangedEventArgs>();
        public static event EventHandler<PropertyChangedEventArgs> PropertyChanged
        {
            add { propertyChangedSource.Subscribe(value); }
            remove { propertyChangedSource.Unsubscribe(value); }
        }

        private static void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            propertyChangedSource.Raise(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static IFilePicker filePicker = null;
        public static IFilePicker FilePicker => filePicker ?? (filePicker = DependencyService.Get<IFilePicker>());

        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        private static void UpdateDeckPath(object sender, DeckChangedEventArgs args)
        {
            if(args is PathChangedEventArgs pathChange)
            {
                AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, pathChange.Path);
            }
        }
    }
}