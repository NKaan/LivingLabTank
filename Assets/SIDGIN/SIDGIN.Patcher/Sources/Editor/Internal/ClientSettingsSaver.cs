using SIDGIN.Common.Editors;
using SIDGIN.Patcher.Client;
using UnityEditor;

namespace SIDGIN.Patcher.Editors
{
    public static class ClientSettingsSaver
    {
        public static async void SaveClientSettings()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>(); 
            var patcherClientSettings = EditorSettingsLoader.Get<ClientSettings>(true);
            if (settingsData.SelectedDefinition == null)
            {
                patcherClientSettings.storageName = settingsData.selectedStorageModule;
                AssetDatabase.SaveAssets();
                return;
            }
            var selectedDefinition = settingsData.SelectedDefinition;
            var storageApi = Storages.Storage.Get();
            patcherClientSettings.appId = await storageApi.GetVersionsLink();
            patcherClientSettings.storageName = settingsData.selectedStorageModule;
            var assetBundleData = EditorSettingsLoader.Get<BuildSettingsCacheData>().GetSelectedData();
            patcherClientSettings.offlineMode = settingsData.offlineMode;
            patcherClientSettings.Save();
            AssetDatabase.SaveAssets();
        }
    }
}
