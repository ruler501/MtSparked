using System;

namespace Gatherer.FilePicker
{
    public class FilePickerEventArgs : EventArgs
    {
        public byte[] Contents { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public FilePickerEventArgs(string filePath)
            : this(filePath, null)
        {
        }

        public FilePickerEventArgs(string filePath, string fileName)
            : this(filePath, fileName, null)
        {
        }

        public FilePickerEventArgs(string filePath, string fileName, byte[] contents)
        {
            this.FilePath = filePath;
            this.FileName = fileName;
            this.Contents = contents;
        }
    }
}