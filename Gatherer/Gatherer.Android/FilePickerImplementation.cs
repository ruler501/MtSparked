using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Runtime;
using Gatherer.FilePicker;
using Java.IO;

[assembly: Xamarin.Forms.Dependency(typeof(Gatherer.Droid.FilePickerImplementation))]
namespace Gatherer.Droid
{
    using File = Java.IO.File;
    using Uri = Android.Net.Uri;

    /// <summary>
    /// Implementation for Feature
    /// </summary>
    [Preserve(AllMembers = true)]
    public class FilePickerImplementation : IFilePicker
    {
        private readonly Context context;
        private int requestId;
        private TaskCompletionSource<FileData> completionSource;

        public FilePickerImplementation()
        {
            this.context = Application.Context;
        }

        public async Task<FileData> SaveFileAs(byte[] dataToSave, string suggestedFileName = null)
        {
            int id = this.GetRequestId();

            TaskCompletionSource<FileData> taskCompletionSource = new TaskCompletionSource<FileData>(id);

            if (Interlocked.CompareExchange(ref this.completionSource, taskCompletionSource, null) != null)
                throw new InvalidOperationException("Only one operation can be active at a time");
            
            Intent pickerIntent = new Intent(this.context, typeof(FilePickerActivity));
            pickerIntent.SetFlags(ActivityFlags.NewTask);
            pickerIntent.PutExtra(FilePickerActivity.SAVING_KEY, true);
            pickerIntent.PutExtra(FilePickerActivity.TITLE_KEY, suggestedFileName);
            pickerIntent.PutExtra(FilePickerActivity.DATA_KEY, dataToSave);

            this.context.StartActivity(pickerIntent);

            FilePickerActivity.FilePickCancelled += this.OnCancelled;
            FilePickerActivity.FilePicked += this.OnCompleted;
            

            return await this.completionSource.Task; ;
        }

        public async Task<FileData> OpenFileAs()
        {
            int id = this.GetRequestId();

            TaskCompletionSource<FileData> taskCompletionSource = new TaskCompletionSource<FileData>(id);

            if (Interlocked.CompareExchange(ref this.completionSource, taskCompletionSource, null) != null)
                throw new InvalidOperationException("Only one operation can be active at a time");

            Intent pickerIntent = new Intent(this.context, typeof(FilePickerActivity));
            pickerIntent.SetFlags(ActivityFlags.NewTask);
            pickerIntent.PutExtra(FilePickerActivity.SAVING_KEY, false);

            this.context.StartActivity(pickerIntent);

            FilePickerActivity.FilePickCancelled += this.OnCancelled;
            FilePickerActivity.FilePicked += this.OnCompleted;


            return await this.completionSource.Task; ;
        }

        public void SaveFile(byte[] fileContents, string path)
        {
            if (path.StartsWith("content://"))
            {
                this.WriteData(Android.Net.Uri.Parse(path), fileContents);
            }
            else
            {
                System.IO.File.WriteAllBytes(path, fileContents);
            }
        }

        public byte[] OpenFile(string path)
        {
            if (path.StartsWith("content://"))
            {
                return this.ReadData(Android.Net.Uri.Parse(path));
            }
            else
            {
                return System.IO.File.ReadAllBytes(path);
            }
        }

        public bool PathExists(string path)
        {
            if(path is null)
            {
                return false;
            }
            if (path.StartsWith("content://"))
            {

                Context ctx = this.context;
                try
                {
                    ContentResolver cr = ctx.ContentResolver;
                    ICursor cursor = cr.Query(Android.Net.Uri.Parse(path), null, null, null, null);
                    return (!(cursor is null) && cursor.MoveToFirst());
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return System.IO.File.Exists(path);
            }
        }

        public void ShareFile(string path, string fileType = "text/*")
        {
            Intent sharingIntent = new Intent(Intent.ActionSend);
            sharingIntent.SetType(fileType);
            sharingIntent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(path));

            this.context.StartActivity(Intent.CreateChooser(sharingIntent, "Share Deck With"));
        }

        public void ReleaseFile(string path)
        {
            if (path.StartsWith("content://"))
            {
                this.context.ContentResolver.ReleasePersistableUriPermission(Android.Net.Uri.Parse(path),
                                                                             ActivityFlags.GrantReadUriPermission |
                                                                              ActivityFlags.GrantWriteUriPermission);
            }
        }

        byte[] ReadData(Android.Net.Uri uri)
        {
            Context ctx = this.context;

            ContentResolver cr = ctx.ContentResolver;

            byte[] buffer = new byte[16 * 1024];
            using (Stream stream = cr.OpenInputStream(uri))
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        void WriteData(Android.Net.Uri uri, byte[] data)
        {
            Context ctx = this.context;

            ContentResolver cr = ctx.ContentResolver;

            using (Stream stream = cr.OpenOutputStream(uri, "w"))
            {
                stream.Write(data, 0, data.Length);
            }
        }

        private int GetRequestId()
        {
            int id = this.requestId;

            if (this.requestId == int.MaxValue) this.requestId = 0;
            else this.requestId++;

            return id;
        }

        public void OnCompleted(object sender, FilePickerEventArgs args)
        {
            TaskCompletionSource<FileData> tcs = Interlocked.Exchange(ref this.completionSource, null);

            FilePickerActivity.FilePicked -= this.OnCompleted;
            FilePickerActivity.FilePickCancelled -= this.OnCancelled;

            tcs?.SetResult(new FileData(args.FilePath, args.FileName, args.Contents));
        }

        public void OnCancelled(object sender, EventArgs args)
        {
            TaskCompletionSource<FileData> tcs = Interlocked.Exchange(ref this.completionSource, null);

            FilePickerActivity.FilePicked -= this.OnCompleted;
            FilePickerActivity.FilePickCancelled -= this.OnCancelled;

            tcs?.SetResult(null);
        }
    }
}