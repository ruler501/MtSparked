using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using Acr.UserDialogs;
using Xamarin.Forms;

namespace Gatherer.Droid
{
    [Activity(Label = "Gatherer", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            var prepopulated = "cards.db.cache";
            var realmDB = "cards.db";
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            if (!File.Exists(Path.Combine(documentsPath, realmDB)))
            {
                using(var db = Assets.Open(prepopulated))
                {
                    using (var dest = File.Create(Path.Combine(documentsPath, realmDB)))
                    {
                        db.CopyTo(dest);
                    }
                }
            }

            //Forms.SetFlags("FastRenderers_Experimental");
            Forms.Init(this, bundle);

            UserDialogs.Init(this);

            LoadApplication(new App());
        }
    }
}

