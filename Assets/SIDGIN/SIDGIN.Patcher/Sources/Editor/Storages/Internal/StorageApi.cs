using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System;
using System.Net;
using SIDGIN.Patcher.Storages;

namespace SIDGIN.Patcher.Editors.Storages
{
    internal interface IStorageApi
    {
        Task<string> UploadFile(string filePath);
        Task<string> UploadData(string data, string fileName);
        Task<List<FileMeta>> GetListFileMeta();
        Task<string> GetVersionsLink();
        Task DeleteFile(string id);
        Task DownloadFile(string id, string filePath);
        Task<string> DownloadData(string id);
        
    }

    public abstract class StorageApiEditor : IStorageApi
    {
        public event System.Action<ProgressData> onProgressChanged;
        public long toRecieveBytes;
        StorageControl storageControl;
        public StorageApiEditor()
        {
            storageControl = new StorageControl();
            storageControl.onProgressChanged += ProgressChanged;
        }
        public abstract Task<string> UploadFile(string filePath);
        public abstract Task<string> UploadFile(string filePath, string fileName);
        public abstract Task<string> UploadData(string data, string fileName);
        public abstract Task<string> GetVersionsLink();
        public abstract Task<List<FileMeta>> GetListFileMeta();
        public abstract Task DeleteFile(string id);
        public abstract Task<string> DownloadDataByName(string name);
        public abstract Task<string> GetFileIdByName(string name);
        protected void ProgressChanged(ProgressData data)
        {
            if (onProgressChanged != null)
                onProgressChanged.Invoke(data);
        }
        public virtual Task DownloadFile(string id, string filePath)
        {
            return storageControl.DownloadFile(id, filePath, "");
        }

        public virtual Task<string> DownloadData(string id)
        {
            return storageControl.DownloadData(id);
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