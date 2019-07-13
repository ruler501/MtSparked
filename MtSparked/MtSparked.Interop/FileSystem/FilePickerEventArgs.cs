using System;

namespace MtSparked.Interop.FileSystem {
    public class FilePickerEventArgs : EventArgs {

        public byte[] Contents { get; private set; }

        public string FileName { get; private set; }

        public string FilePath { get; private set; }

        public FilePickerEventArgs(string filePath)
            : this(filePath, null)
        {}

        public FilePickerEventArgs(string filePath, string fileName)
            : this(filePath, fileName, null)
        {}

        public FilePickerEventArgs(string filePath, string fileName, byte[] contents) {
            this.FilePath = filePath;
            this.FileName = fileName;
            this.Contents = contents;
        }

    }
}