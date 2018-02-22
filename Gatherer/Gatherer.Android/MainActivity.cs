using System;
using System.Collections.Generic;
using System.Security.Cryptography;

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

            var start = DateTime.Now;
            string hash = null;
            var prepopulated = "cards.db.cache";
            var realmDB = "cards.db";
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var finalPath = Path.Combine(documentsPath, realmDB);
            bool toReplace = !File.Exists(finalPath);
            if (!toReplace)
            {
                using(var md5 = MD5.Create())
                using(var stream = File.OpenRead(finalPath))
                {
                    HashSet<string> sums = new HashSet<string>() { "5B-9C-27-0B-A2-A2-E2-8A-B0-B4-AA-DC-05-C0-09-B8" };
                    byte[] result = md5.ComputeHash(stream);
                    hash = BitConverter.ToString(result);
                    if (sums.Contains(hash))
                    {
                        toReplace = true;
                    }
                }
            }
            var time = DateTime.Now - start;
            if (toReplace)
            {
                using(var db = Assets.Open(prepopulated))
                using (var dest = File.Create(finalPath))
                {
                    db.CopyTo(dest);
                }
                
            }

            //Forms.SetFlags("FastRenderers_Experimental");
            Forms.Init(this, bundle);

            UserDialogs.Init(this);

            LoadApplication(new App());
        }
    }
}

