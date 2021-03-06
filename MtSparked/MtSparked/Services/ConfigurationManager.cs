﻿using MtSparked.FilePicker;
using MtSparked.Models;
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

namespace MtSparked.Services
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

        private const string ACTIVE_CUBE_DEF_KEY = "ActiveCubeDefPath";
        public static string DefaultCubeDefPath => Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "temp.cdef");
        private static string activeCubePath = null;
        public static string ActiveCubePath
        {
            get
            {
                if(activeCubePath is null)
                {
                    ActiveCubePath = AppSettings.GetValueOrDefault(ACTIVE_CUBE_DEF_KEY, DefaultCubeDefPath);
                }
                return activeCubePath;
            }
            set
            {
                if (activeCubePath != value)
                {
                    FilePicker.ReleaseFile(activeCubePath);

                    if (FilePicker.PathExists(value))
                    {
                        activeCubePath = value;
                    }
                    else
                    {
                        activeCubePath = value;
                        AppSettings.AddOrUpdateValue(ACTIVE_CUBE_DEF_KEY, value);
                    }
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

        private static string DATABASE_VERSION_KEY = "DatabaseVersion";
        public static int DatabaseVersion
        {
            get => AppSettings.GetValueOrDefault(DATABASE_VERSION_KEY, 0);
            set => AppSettings.AddOrUpdateValue(DATABASE_VERSION_KEY, value);
        }
        public const int CurrentDatabaseVersion = 2;

        private static string SORT_CRITERIA_KEY = "SortCriteria";
        public static string SortCriteria
        {
            get => AppSettings.GetValueOrDefault(SORT_CRITERIA_KEY, "Cmc");
            set
            {
                AppSettings.AddOrUpdateValue(SORT_CRITERIA_KEY, value);
                OnPropertyChanged();
            }
        }

        private static string COUNT_BY_GROUP_KEY = "CountByGroup";
        public static bool CountByGroup
        {
            get => AppSettings.GetValueOrDefault(COUNT_BY_GROUP_KEY, false);
            set
            {
                AppSettings.AddOrUpdateValue(COUNT_BY_GROUP_KEY, value);
                OnPropertyChanged();
            }
        }

        private static string DESCENDING_SORT_KEY = "DescendingSort";
        public static bool DescendingSort
        {
            get => AppSettings.GetValueOrDefault(DESCENDING_SORT_KEY, false);
            set
            {
                AppSettings.AddOrUpdateValue(DESCENDING_SORT_KEY, value);
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