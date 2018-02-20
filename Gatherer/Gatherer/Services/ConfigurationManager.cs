using Gatherer.Models;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
                    string activeDeckName = AppSettings.GetValueOrDefault(ACTIVE_DECK_KEY, null);
                    if(activeDeckName is null)
                    {
                        var documentsPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                        ActiveDeck = new Deck(Path.Combine(documentsPath, DEFAULT_DECK_PATH));
                    }
                    else
                    {
                        ActiveDeck = new Deck(activeDeckName);
                    }
                }
                return activeDeck;
            }
            set
            {
                activeDeck = value;
                if(activeDeck is null)
                {
                    var documentsPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    activeDeck = new Deck(Path.Combine(documentsPath, DEFAULT_DECK_PATH));
                }

                if (File.Exists(activeDeck.StoragePath))
                {
                    AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, activeDeck.StoragePath);
                }
                else
                {
                    var documentsPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    activeDeck.StoragePath = Path.Combine(documentsPath, DEFAULT_DECK_PATH);
                    AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, activeDeck.StoragePath);
                }
            } }

        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = string.Empty;

        #endregion


        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SettingsKey, value);
            }
        }
    }
}