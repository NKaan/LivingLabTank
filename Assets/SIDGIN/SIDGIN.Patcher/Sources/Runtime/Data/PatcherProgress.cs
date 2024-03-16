namespace SIDGIN.Patcher.Client
{
    public struct DownloadProgress
    {
        public float progress;
        public string status;
        public string speed;
        public string downloadedSize;
        public string targetSize;
    }
    public class PatcherProgress
    {
        public float progress;
        public string status;
        public DownloadProgress downloadProgress;
    }
}
