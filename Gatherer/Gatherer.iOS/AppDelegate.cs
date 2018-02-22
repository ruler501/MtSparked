using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Foundation;
using Syncfusion.SfDataGrid.XForms.iOS;
using UIKit;

namespace Gatherer.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            
            SfDataGridRenderer.Init();

            LoadApplication(new Gatherer.App());

            var prepopulated = "cards.db.cache";
            var realmDB = "cards.db";
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            if (!File.Exists(Path.Combine(documentsPath, realmDB)))
            {
                File.Copy(prepopulated, Path.Combine(documentsPath, realmDB));
            }

            return base.FinishedLaunching(app, options);
        }
    }
}
