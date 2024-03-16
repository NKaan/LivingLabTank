using UnityEngine;
namespace SIDGIN.Patcher.Client
{
    public class ClientSettings : ScriptableObject
    {
        public string appId;
        public string storageName;
        public bool offlineMode;
        public bool syncMainBuild = true;
        public string languageKey;
        public static ClientSettings Get()
        {
            var patcherClinetSettings = Resources.Load<ClientSettings>("ClientSettings");
            if (patcherClinetSettings == null)
                throw new System.ApplicationException("ClientSettings asset not found!");
            return patcherClinetSettings;
        }
    }
}