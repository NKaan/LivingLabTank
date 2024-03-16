using UnityEngine;
namespace SIDGIN.Patcher.Editors.Storages
{
    public class GoogleSettingsData : ScriptableObject
    {
        [SerializeField]
        public string sharedFolder = "patches";
    }
}