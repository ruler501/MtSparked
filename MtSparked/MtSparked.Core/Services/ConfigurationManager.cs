using Autofac;
using MtSparked.Interop.Services;
using MtSparked.Interop.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using WeakEvent;

namespace MtSparked.Core.Services
{
    public static class ConfigurationManager {

        public const int CurrentDatabaseVersion = 2;

        static ConfigurationManager() {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assemblies = currentDomain.GetAssemblies();
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(assemblies);
            Container = builder.Build();

            AppSettings = Container.Resolve<ISettings>();
        }

        public static Autofac.IContainer Container { get; }

        public static string DefaultDeckPath => Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "temp.jdec");
        public static string DefaultTempDecPath => Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "temp.dec");
        public static string DefaultCubeDefPath => Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "temp.cdef");

        private const string ACTIVE_DECK_KEY = "ActiveDeck";
        private static Deck activeDeck = null;
        public static Deck ActiveDeck {
            get {
                if(activeDeck is null) {
                    string activeDeckPath = AppSettings.GetValueOrDefault(ACTIVE_DECK_KEY, null);
                    if(activeDeckPath is null || !FilePicker.PathExists(activeDeckPath)) {
                        ActiveDeck = Deck.FromJdec(DefaultDeckPath);
                    } else {
                        ActiveDeck = Deck.FromJdec(activeDeckPath);
                    }
                }
                return activeDeck;
            }
            set {
                string path = null;
                if (!(activeDeck is null)) {
                    activeDeck.ChangeEvent -= ConfigurationManager.UpdateDeckPath;
                    path = activeDeck.StoragePath;
                }
                activeDeck = value;
                if(activeDeck is null) {
                    activeDeck = Deck.FromJdec(DefaultDeckPath);
                }

                if (FilePicker.PathExists(activeDeck.StoragePath)) {
                    AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, activeDeck.StoragePath);
                } else {
                    activeDeck.StoragePath = DefaultDeckPath;
                    AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, activeDeck.StoragePath);
                }
                if (!(activeDeck is null)) {
                    activeDeck.ChangeEvent += ConfigurationManager.UpdateDeckPath;
                    OnPropertyChanged();
                }
            }
        }

        private const string PRETTY_PRINT_JDEC_KEY = "PrettyPrintJDec";
        public static bool PrettyPrintJDec {
            get { return AppSettings.GetValueOrDefault(PRETTY_PRINT_JDEC_KEY, true); }
            set { AppSettings.AddOrUpdateValue(PRETTY_PRINT_JDEC_KEY, value); }
        }

        private const string ACTIVE_CUBE_DEF_KEY = "ActiveCubeDefPath";
        private static string activeCubePath = null;
        public static string ActiveCubePath {
            get {
                if(activeCubePath is null) {
                    ActiveCubePath = AppSettings.GetValueOrDefault(ACTIVE_CUBE_DEF_KEY, DefaultCubeDefPath);
                }
                return activeCubePath;
            }
             set {
                if (activeCubePath != value) {
                    FilePicker.ReleaseFile(activeCubePath);
                    activeCubePath = value;

                    // TODO: Review this logic
                    if (!FilePicker.PathExists(value)) {
                        AppSettings.AddOrUpdateValue(ACTIVE_CUBE_DEF_KEY, value);
                    }
                    // TODO: Why does this not call onPropertyChanged?
                }
            }
        }

        private static string SHOW_UNIQUE_KEY = "ShowUnique";
        public static bool ShowUnique {
            get { return AppSettings.GetValueOrDefault(SHOW_UNIQUE_KEY, false); }
            set {
                AppSettings.AddOrUpdateValue(SHOW_UNIQUE_KEY, value);
                OnPropertyChanged();
            }
        }

        private static string DATABASE_VERSION_KEY = "DatabaseVersion";
        public static int DatabaseVersion {
            get { return AppSettings.GetValueOrDefault(DATABASE_VERSION_KEY, 0); }
            set { AppSettings.AddOrUpdateValue(DATABASE_VERSION_KEY, value); }
        }

        private static string SORT_CRITERIA_KEY = "SortCriteria";
        public static string SortCriteria {
            get { return AppSettings.GetValueOrDefault(SORT_CRITERIA_KEY, "Cmc"); }
            set {
                AppSettings.AddOrUpdateValue(SORT_CRITERIA_KEY, value);
                OnPropertyChanged();
            }
        }

        private static string COUNT_BY_GROUP_KEY = "CountByGroup";
        public static bool CountByGroup {
            get { return AppSettings.GetValueOrDefault(COUNT_BY_GROUP_KEY, false); }
            set {
                AppSettings.AddOrUpdateValue(COUNT_BY_GROUP_KEY, value);
                OnPropertyChanged();
            }
        }

        private static string DESCENDING_SORT_KEY = "DescendingSort";
        public static bool DescendingSort {
            get { return AppSettings.GetValueOrDefault(DESCENDING_SORT_KEY, false); }
            set {
                AppSettings.AddOrUpdateValue(DESCENDING_SORT_KEY, value);
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged
        private static WeakEventSource<PropertyChangedEventArgs> propertyChangedSource = new WeakEventSource<PropertyChangedEventArgs>();
        public static event EventHandler<PropertyChangedEventArgs> PropertyChanged {
            add { propertyChangedSource.Subscribe(value); }
            remove { propertyChangedSource.Unsubscribe(value); }
        }

        private static void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            propertyChangedSource.Raise(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static IFilePicker filePicker = null;
        public static IFilePicker FilePicker => filePicker ?? (filePicker = Container.Resolve<IFilePicker>());

        private static ISettings AppSettings { get; }

        private static void UpdateDeckPath(object sender, DeckChangedEventArgs args) {
            if(args is PathChangedEventArgs pathChange) {
                AppSettings.AddOrUpdateValue(ACTIVE_DECK_KEY, pathChange.Path);
            }
        }

    }
}