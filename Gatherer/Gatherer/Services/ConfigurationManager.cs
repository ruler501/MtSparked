using Gatherer.FilePicker;
using Gatherer.Models;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace Gatherer.Services
{
    public static class ConfigurationManager
    {
        private const string ACTIVE_DECK_KEY = "ActiveDeck";
        private const string DEFAULT_DECK_PATH = "temp.jdec";
        private static Deck activeDeck = null;
        public static Deck ActiveDeck {
            get
            {
                if(activeDeck is null)
                {
                    string activeDeckPath = AppSettings.GetValueOrDefault(ACTIVE_DECK_KEY, null);
                    if(activeDeckPath is null)
                    {
                        var documentsPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                        ActiveDeck = new Deck(Path.Combine(documentsPath, DEFAULT_DECK_PATH));
                    }
                    else
                    {
                        ActiveDeck = new Deck(activeDeckPath);
                    }
                }
                return activeDeck;
            }
            set
            {
                if (!(activeDeck is null)) activeDeck.ChangeEvent -= ConfigurationManager.UpdateDeckPath;
                activeDeck = value;
                if(activeDeck is null)
                {
                    var documentsPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    activeDeck = new Deck(Path.Combine(documentsPath, DEFAULT_DECK_PATH));
                }

                if (FilePicker.PathExists(activeDeck.StoragePath))
                {
                    AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, activeDeck.StoragePath);
                }
                else
                {
                    var documentsPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    activeDeck.StoragePath = Path.Combine(documentsPath, DEFAULT_DECK_PATH);
                    AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, activeDeck.StoragePath);
                }
                if (!(activeDeck is null)) activeDeck.ChangeEvent += ConfigurationManager.UpdateDeckPath;
            }
        }

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