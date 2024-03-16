

using SIDGIN.Patcher.Client;

namespace SIDGIN.Patcher.Storages
{
    internal static class Storage
    {
        const string SUPPORTED_STORAGE_TYPES = "Google, Http";
        internal static StorageApi Get()
        {
            var patcherClientSettings = ClientSettings.Get();

#if SGPATCHER_GOOGLE
            if (patcherClientSettings.storageName == "Google")
            {
                var googleStorage = new GoogleDriveApi();
                return googleStorage;
            }
            else  
#endif
            if(patcherClientSettings.storageName == "Http")
            {
                return new HttpApi();
            }
            else if(patcherClientSettings.storageName == "AmazonS3")
            {
                return new HttpApi();
            }
            else
            {
                throw new System.InvalidOperationException("Unsupported storage type. Check configuration file. Supported storage types: " + SUPPORTED_STORAGE_TYPES);
            }
            
        }
    }
}