using System;

using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using System.IO;
using Acr.UserDialogs;
using Xamarin.Forms;
using MtSparked.Interop.Services;
using MtSparked.UI;

namespace MtSparked.Platforms.Droid {
    [Activity(Label = "MtSparked", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);
            // TODO #98: Fix Android Tabbar and Toolbar
            // TabLayoutResource = Resource.Layout.Tabbar;
            // ToolbarResource = Resource.Layout.Toolbar;

            /* TODO #99: Provide Couchbase Lite Database Service
            DateTime start = DateTime.Now;
            const string prepopulated = "cards.db.cache";
            const string realmDB = "cards.db";
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string finalPath = Path.Combine(documentsPath, realmDB);
            bool toReplace = !File.Exists(finalPath) ||
                             ConfigurationManager.DatabaseVersion != ConfigurationManager.CurrentDatabaseVersion;

            if (toReplace) {
                using(Stream db = this.Assets.Open(prepopulated))
                using (FileStream dest = File.Create(finalPath)) {
                    db.CopyTo(dest);
                }
                
            }
            */

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);

#if !DEBUG
            AppCenter.Start("7cd326d7-e39b-4243-9a5a-1e3da85441fd",
                   typeof(Analytics), typeof(Crashes));
#endif
            Forms.Init(this, bundle);

            UserDialogs.Init(this);

            this.LoadApplication(new App());

            XFGloss.Droid.Library.Init(this, bundle);
        }
    }
}

