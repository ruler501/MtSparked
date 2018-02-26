using System;
using System.Collections.Generic;
using System.Text;

namespace MtSparked.FilePicker
{
    public class FileData
    {
        private string fileName;
        private string filePath;
        private byte[] contents;

        public FileData(string filePath, string fileName, byte[] contents)
        {
            this.filePath = filePath;
            this.fileName = fileName;
            this.contents = contents;
        }

        public string FileName => fileName;
        public string FilePath => filePath;
        public byte[] Contents => contents;
    }
}
