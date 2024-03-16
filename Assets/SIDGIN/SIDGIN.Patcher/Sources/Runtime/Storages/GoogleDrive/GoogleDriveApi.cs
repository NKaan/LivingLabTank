using System.Threading.Tasks;

namespace SIDGIN.Patcher.Storages
{
    using Client;

    public class GoogleDriveApi : StorageApi
    {
        #region Download
        public override async Task DownloadFile(string id, string filePath, string targetHash)
        {
            PrepareCertificate();
            var googleHttpProvider = new GoogleHttpDownloadClient();
            googleHttpProvider.ProgressChanged += OnDownloadProgressChanged;
            await googleHttpProvider.DownloadFileAsync(id, filePath, targetHash);
        }


        public override async Task<string> DownloadData(string id)
        {
            PrepareCertificate();
            var googleHttpProvider = new GoogleHttpDownloadClient();
            return await googleHttpProvider.DownloadDataAsync(id);
        }
        #endregion

    }
}