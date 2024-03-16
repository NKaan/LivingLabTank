using SIDGIN.Patcher.Storages;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace SIDGIN.Patcher.Editors.Storages
{
    public class StorageControlEditor : IStorageApi
    {
        StorageApiEditor storage;
        public event System.Action<ProgressData> onProgressChanged;
        public StorageControlEditor()
        {
            storage = Storage.Get();
            storage.onProgressChanged += OnProgressChanged;
        }
        void OnProgressChanged(ProgressData data)
        {
            onProgressChanged?.Invoke(data);
        }
        public async Task<string> UploadFile(string filePath)
        {
            return await storage.UploadFile(filePath);
        }

        public async Task<string> UploadData(string data, string fileName)
        {
            return await storage.UploadData(data, fileName);
        }
        public Task DownloadFile(string id, string filePath)
        {
            return DownloadFile(id, filePath, 0);
        }
        public async Task DownloadFile(string id, string filePath, long fileSize)
        {
            storage.toRecieveBytes = fileSize;
            await storage.DownloadFile(id, filePath);
        }

        public async Task<string> DownloadData(string id)
        {
            return await storage.DownloadData(id);
        }

        public Task<List<FileMeta>> GetListFileMeta()
        {
            return storage.GetListFileMeta();
        }

        public Task DeleteFile(string id)
        {
            return storage.DeleteFile(id);
        }

        public Task<string> GetVersionsLink()
        {
            return storage.GetVersionsLink();
        }


    }
}
