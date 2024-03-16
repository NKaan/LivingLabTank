using System.Threading.Tasks;

namespace SIDGIN.Patcher.Storages
{
    internal class HttpApi : StorageApi
    {
        #region Download
        public override async Task DownloadFile(string link, string filePath, string targetHash)
        {
            PrepareCertificate();
            var downloader = new HttpDownloadClient();
            downloader.ProgressChanged += OnDownloadProgressChanged;
            await downloader.DownloadFileAsync(link, filePath, targetHash);
        }

        public override async Task<string> DownloadData(string link)
        {
            PrepareCertificate();
            var downloader = new HttpDownloadClient();
            return await downloader.DownloadDataAsync(link);
        }
        #endregion

    }

}