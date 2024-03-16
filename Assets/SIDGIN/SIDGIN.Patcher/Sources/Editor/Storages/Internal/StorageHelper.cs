namespace SIDGIN.Patcher.Editors.Storages
{
    using SIDGIN.Common.Editors;
    using SIDGIN.Patcher.Editors;

    internal static class Storage
    {
#if SGPATCHER_GOOGLE
        const string SUPPORTED_STORAGE_TYPES = "Google, Http, AmazonS3";
#else
        const string SUPPORTED_STORAGE_TYPES = "Http, AmazonS3";
#endif
        internal static StorageApiEditor Get()
        {
            var settings = EditorSettingsLoader.Get<SettingsData>();
#if SGPATCHER_GOOGLE
            if (settings.selectedStorageModule == "Google")
            {
                var googleStorage = new GoogleDriveApiEditor();
                return googleStorage;
            }
            else
#endif

            if (settings.selectedStorageModule == "AmazonS3")
            {
                var amazonStorage = new AmazonApiEditor();
                return amazonStorage;
            }
            else  if(settings.selectedStorageModule == "Http")
            {
                return new FtpApi();
            }
            else
            {
                throw new System.InvalidOperationException("Unsupported storage type. Check configuration file. Supported storage types: " + SUPPORTED_STORAGE_TYPES);
            }
            
        }
    }
}