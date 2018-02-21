using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Gatherer.FilePicker;
using Java.IO;

// Credit to LeoJHarris. Code originated with https://github.com/LeoJHarris/FilePicker
namespace Gatherer.Droid
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [Preserve(AllMembers = true)]
    public class FilePickerActivity : Activity
    {
        public const string SAVING_KEY = "Saving";
        public const string TITLE_KEY = "Title";
        public const string DATA_KEY = "Data";

        private const int READ_REQUEST_CODE = 42;
        private const int WRITE_REQUEST_CODE = 84;
        private Context context;
        private byte[] data = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.context = Application.Context;
            bool saving = Intent.GetBooleanExtra(SAVING_KEY, true);
            int requestCode;
            Intent intent;
            if (saving)
            {
                intent = new Intent(Intent.ActionCreateDocument);
                intent.PutExtra(Intent.ExtraTitle, Intent.GetStringExtra(TITLE_KEY));
                requestCode = WRITE_REQUEST_CODE;
                this.data = Intent.GetByteArrayExtra(DATA_KEY);
            }
            else
            {
                intent = new Intent(Intent.ActionOpenDocument);
                requestCode = READ_REQUEST_CODE;
            }
            intent.AddFlags(ActivityFlags.GrantPersistableUriPermission |
                            ActivityFlags.GrantReadUriPermission |
                            ActivityFlags.GrantWriteUriPermission);
            intent.SetType("*/*");
            intent.AddCategory(Intent.CategoryOpenable);

            try
            {
                this.StartActivityForResult(Intent.CreateChooser(intent, "Select file"), requestCode);
            }
            catch (Exception exAct)
            {
                System.Diagnostics.Debug.Write(exAct);
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Canceled)
            {
                // Notify user file picking was cancelled.
                OnFilePickCancelled();
                this.Finish();
            }
            else if (requestCode == READ_REQUEST_CODE && resultCode == Result.Ok)
            {
                try
                {
                    ActivityFlags takeFlags = data.Flags & (ActivityFlags.GrantReadUriPermission |
                                                            ActivityFlags.GrantWriteUriPermission);
                    this.context.ContentResolver.TakePersistableUriPermission(data.Data, takeFlags);

                    FilePickerEventArgs args = new FilePickerEventArgs(data.Data.ToString(), this.GetFileName(data.Data), this.ReadData(data.Data));
                    OnFilePicked(args);
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Debug.Write(exc);
                    // Notify user file picking failed.
                    OnFilePickCancelled();
                }
                finally
                {
                    this.Finish();
                }
            }
            else if (requestCode == WRITE_REQUEST_CODE && resultCode == Result.Ok)
            {
                try
                {
                    FilePickerEventArgs args = new FilePickerEventArgs(data.Data.ToString(), this.GetFileName(data.Data), this.data);
                    this.WriteData(data.Data);
                    OnFilePicked(args);
                }
                catch (Exception)
                {
                    // Notify user file picking failed.
                    OnFilePickCancelled();
                }
                finally
                {
                    this.Finish();
                }
            }
        }

        string GetFileName(Android.Net.Uri uri)
        {
            Context ctx = this.context;
            string[] projection = { MediaStore.MediaColumns.DisplayName };

            ContentResolver cr = ctx.ContentResolver;
            string name = string.Empty;
            ICursor metaCursor = cr.Query(uri, projection, null, null, null);

            if (metaCursor != null)
            {
                try
                {
                    if (metaCursor.MoveToFirst())
                    {
                        name = metaCursor.GetString(0);
                    }
                }
                finally
                {
                    metaCursor.Close();
                }
            }

            return name;
        }

        byte[] ReadData(Android.Net.Uri uri)
        {
            Context ctx = this.context;

            ContentResolver cr = ctx.ContentResolver;

            byte[] buffer = new byte[16 * 1024];
            using (Stream stream = cr.OpenInputStream(uri))
            using(MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        void WriteData(Android.Net.Uri uri)
        {
            if (this.data is null) return;


            Context ctx = this.context;

            ContentResolver cr = ctx.ContentResolver;
            
            using (Stream stream = cr.OpenOutputStream(uri, "w"))
            {
                stream.Write(this.data, 0, this.data.Length);
            }
        }

        internal static event EventHandler<FilePickerEventArgs> FilePicked;
        internal static event EventHandler<EventArgs> FilePickCancelled;

        private static void OnFilePickCancelled()
        {
            FilePickCancelled?.Invoke(null, null);
        }

        private static void OnFilePicked(FilePickerEventArgs args)
        {
            FilePicked?.Invoke(null, args);
        }
    }
}