using System.Threading.Tasks;

using MtSparked.Interop.FileSystem;

namespace MtSparked.Interop.Services {
    public interface IFilePicker {

        Task<FileData> SaveFileAs(byte[] dataToSave, string suggestedFileName = null);

        void SaveFile(byte[] fileContents, string path);

        Task<FileData> OpenFileAs();

        byte[] OpenFile(string path);

        bool PathExists(string path);

        void ShareFile(string path, string fileType="text/*");

        void ReleaseFile(string path);

    }
}
