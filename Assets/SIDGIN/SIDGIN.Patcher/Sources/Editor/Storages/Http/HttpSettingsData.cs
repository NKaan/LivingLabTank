namespace SIDGIN.Patcher.Editors.Storages
{
    using UnityEngine;

    public class HttpSettingsData : ScriptableObject
    {
        public enum FTPType
        {
            FTP,
            SFTP
        }
        public FTPType ftpProtocolType;
        public string ftpDomain = "example.com";
        public string ftpUser;
        public string ftpPassword;
        public string ftpPort = "21";
        public bool enableSSL = true;
        public string ftpHomeDirectory;
        public string privateKeyFilePath;
        public string privateKeyPassphrase;
        public string rootLink = "https://example.com/patches/";
    }
}