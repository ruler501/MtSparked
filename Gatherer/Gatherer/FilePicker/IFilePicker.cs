using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gatherer.FilePicker
{
    public interface IFilePicker
    {
        Task<FileData> SaveFileAs(byte[] dataToSave, string suggestedFileName = null);

        void SaveFile(byte[] fileContents, string path);

        Task<FileData> OpenFileAs();

        byte[] OpenFile(string path);

        bool PathExists(string path);

        void ShareFile(string path, string fileType="text/*");

        void ReleaseFile(string path);
    }
}
