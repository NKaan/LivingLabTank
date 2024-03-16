using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    using SIDGIN.Patcher.Client;
    using SIDGIN.Common.Editors;
    public enum InAppBuildTarget
    {
        Android = 13,
        iOS = 9,
        PS4 = 31,
        XboxOne = 33,
        StandaloneOSX = 2,
        StandaloneWindows = 5,
        StandaloneWindows64 = 19,
        StandaloneLinux = 17,
        StandaloneLinux64 = 24,
        StandaloneLinuxUniversal = 25,
        WSAPlayer = 21,
        tvOS = 37,
        Switch = 38,
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        Lumin = 39
#endif
    }
    public enum BundlesCompressType
    {
        LZMA,
        LZ4,
        Uncompressed
    }
    [System.Serializable]
    public class SceneEditorData
    {
        public bool IsActive { get; internal set; }
        public int index;
        public bool enabled;
        public string name;
        public string path;
        public string sharedResourcesKey = "None";
        public string packageName = Consts.MAIN_PACKAGE_NAME;
    }
    [System.Serializable]
    public class SharedFolderData
    {
        public string name;
        public string path;
    }
    [System.Serializable]
    public class SGResourceData
    {
        public string localPath;
        public string bundleName;
    }
    [System.Serializable]
    public class SGResourcePackageData
    {
        public string packageName = Consts.MAIN_PACKAGE_NAME;
        public string localPath;
        public string bundleName;
    }
    [System.Serializable]
    public class BuildSettingsData
    {
        public string definitionName;
        public string gameResourcesFolderPath;
        public string scenesFolderPath;
        public InAppBuildTarget buildTarget;
        public BundlesCompressType compressType = BundlesCompressType.LZ4;
        public List<SceneEditorData> scenes = new List<SceneEditorData>();
        public List<SharedFolderData> sharedFolders = new List<SharedFolderData>();
        public List<SGResourceData> sgResources = new List<SGResourceData>();
        public List<SGResourcePackageData> sgResourcePackage = new List<SGResourcePackageData>();
        public List<string> packages = new List<string>(){ Consts.MAIN_PACKAGE_NAME };
        public List<PackageUploadTask> packageUploadTasks = new List<PackageUploadTask>();
    }
    public class BuildSettingsCacheData : ScriptableObject
    {
        [SerializeField]
        List<BuildSettingsData> settingsByDefinition = new List<BuildSettingsData>();
       

        public BuildSettingsData GetSelectedData()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            if (settingsData.SelectedDefinition == null)
                return null;
            var selectedData = settingsByDefinition.FirstOrDefault(x => x.definitionName == settingsData.selectedDefinition);
            if(selectedData == null)
            {
                selectedData = new BuildSettingsData
                {
                    definitionName = settingsData.selectedDefinition,
                };
                settingsByDefinition.Add(selectedData);
            }
            return selectedData;
        }
        public void RemoveSettings(string definitionName)
        {
            settingsByDefinition.RemoveAll(x => x.definitionName == definitionName);
        }
      
    }
}
