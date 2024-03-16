using UnityEngine;
using UnityEngine.Serialization;

namespace SIDGIN.Patcher.Editors
{
    [System.Serializable]
    public class Definition
    {
        [SerializeField]
        string name;
        public string Name { get { return name; } }


        [SerializeField]
        string storageName;

        public string StorageName { get { return storageName; } }

        public string toDeleteFromCacheVersion;
        public string versionsFileId;
        public Definition(string name, string storageName)
        {
            this.name = name;
            this.storageName = storageName;
        }
    }
}