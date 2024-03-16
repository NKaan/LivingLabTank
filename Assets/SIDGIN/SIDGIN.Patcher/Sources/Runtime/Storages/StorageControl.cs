using System.Threading.Tasks;
namespace SIDGIN.Patcher.Storages
{
    public class StorageControl: IStorageApi
    {
        StorageApi storage;
        public event System.Action<ProgressData> onProgressChanged;
        public StorageControl()
        {
            storage = Storage.Get();
            storage.onProgressChanged += OnProgressChanged;
        }
        void OnProgressChanged(ProgressData data)
        {
            onProgressChanged?.Invoke(data);
        }

        public Task DownloadFile(string id, string filePath, string targetHash)
        {
            return DownloadFile(id, filePath, targetHash, 0);
        }
        public async Task DownloadFile(string id, string filePath, string targetHash, long fileSize)
        {
            storage.toRecieveBytes = fileSize;
            await storage.DownloadFile(id, filePath, targetHash);
        }

        public async Task<string> DownloadData(string id)
        {
            return await storage.DownloadData(id);
        }


    }
}
