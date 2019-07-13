namespace MtSparked.Interop.FileSystem {
    public class FileData {

        public FileData(string filePath, string fileName, byte[] contents) {
            this.FilePath = filePath;
            this.FileName = fileName;
            this.Contents = contents;
        }

        public string FileName { get; }
        public string FilePath { get; }
        public byte[] Contents { get; }

    }
}
