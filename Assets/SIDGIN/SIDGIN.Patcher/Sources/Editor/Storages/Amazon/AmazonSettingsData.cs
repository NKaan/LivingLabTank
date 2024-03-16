namespace SIDGIN.Patcher.Editors.Storages
{
    using UnityEngine;

    public class AmazonSettingsData : ScriptableObject
    {
        public string accessKeyId;
        public string secretAccessKey;
        public string bucketName;
        public AmazonEndpoint endpoint;
        [SerializeField]
        public string rootFolder = "patches";

    }
}