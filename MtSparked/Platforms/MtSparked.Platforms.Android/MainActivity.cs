using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using MtSparked.Services;
using CarouselView.FormsPlugin.Android;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using Acr.UserDialogs;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MtSparked.Core.Services;

namespace MtSparked.Droid
{

    [Activity(Label = "MtSparked", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        TextView MsgText { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            var start = DateTime.Now;
            var prepopulated = "cards.db.cache";
            var realmDB = "cards.db";
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var finalPath = Path.Combine(documentsPath, realmDB);
            bool toReplace = !File.Exists(finalPath) ||
                             ConfigurationManager.DatabaseVersion != ConfigurationManager.CurrentDatabaseVersion;

            if (toReplace)
            {
                using(var db = Assets.Open(prepopulated))
                using (var dest = File.Create(finalPath))
                {
                    db.CopyTo(dest);
                }
                
            }

            CarouselViewRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);

            //Forms.SetFlags("FastRenderers_Experimental");
#if !DEBUG
            AppCenter.Start("7cd326d7-e39b-4243-9a5a-1e3da85441fd",
                   typeof(Analytics), typeof(Crashes));
#endif
            Forms.Init(this, bundle);

            UserDialogs.Init(this);

            LoadApplication(new App());

            XFGloss.Droid.Library.Init(this, bundle);
        }
    }
}

