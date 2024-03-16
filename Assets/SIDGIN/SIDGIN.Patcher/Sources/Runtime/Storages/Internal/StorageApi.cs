using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System;
using System.Net;

namespace SIDGIN.Patcher.Storages
{
    internal interface IStorageApi
    {
        Task DownloadFile(string id, string filePath, string targetHash);
        Task<string> DownloadData(string id);
        
    }

    public abstract class StorageApi : IStorageApi
    {
        public event System.Action<ProgressData> onProgressChanged;
        public long toRecieveBytes;
        public abstract Task DownloadFile(string id, string filePath, string targetHash);
        public abstract Task<string> DownloadData(string id);


        protected void ProgressChanged(ProgressData data)
        {
            if (onProgressChanged != null)
                onProgressChanged.Invoke(data);
        }
        protected void OnDownloadProgressChanged(DownloadProgressChanged e)
        {
            if (toRecieveBytes == 0)
                toRecieveBytes = 1;


            ProgressChanged(new ProgressData
            {
                targetBytes = toRecieveBytes,
                downloadedBytes = e.BytesReceived,
                progress = e.BytesReceived / (float)toRecieveBytes,
                bytesPerSecond = e.BytesPerSecond
            });

        }
        bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }
            return isOk;
        }
        protected void PrepareCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        }
    }
}