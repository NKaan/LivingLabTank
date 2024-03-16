using SIDGIN.Common.Editors;
using UnityEditor;
using UnityEngine;
namespace SIDGIN.Patcher.Editors
{
    public class AssetBundlesSettingsView
    {
        SettingsData settingsData;
        BuildSettingsData buildSettings;
        PackagesView packagesView;
        SceneManagerView scenesView;
        SharedResourcesView sharedResourcesView;
        SGResourcesView sgResourcesView;
        public void OnEnable()
        {
            settingsData = EditorSettingsLoader.Get<SettingsData>();
            if (settingsData.SelectedDefinition == null)
                return;
            buildSettings = EditorSettingsLoader.Get<BuildSettingsCacheData>().GetSelectedData();
            
            packagesView = new PackagesView(buildSettings);
            scenesView = new SceneManagerView(buildSettings);
            sharedResourcesView = new SharedResourcesView(buildSettings.sharedFolders);
            sgResourcesView = new SGResourcesView(buildSettings);
        }
        public void OnDraw()
        {
            if (settingsData.SelectedDefinition == null)
            {
                EditorGUILayout.HelpBox("You must create a definition before setting up the assembly.", MessageType.Warning);
                return;
            }
            if (buildSettings == null)
                OnEnable();
            if (buildSettings.definitionName != settingsData.SelectedDefinition.Name)
                OnEnable();

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Compress Type:", GUILayout.Width(130));
            buildSettings.compressType = (BundlesCompressType)EditorGUILayout.EnumPopup(buildSettings.compressType);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("*Build Target:", GUILayout.Width(130));
            buildSettings.buildTarget = (InAppBuildTarget)EditorGUILayout.EnumPopup(buildSettings.buildTarget);
            GUILayout.EndHorizontal();
        
            settingsData.offlineMode = EditorGUILayout.ToggleLeft("Run the game when the Internet connection is disconnected.", settingsData.offlineMode);
            if (EditorGUI.EndChangeCheck())
            {
                EditorSettingsLoader.Get<BuildSettingsCacheData>().Save();
                ClientSettingsSaver.SaveClientSettings();
                settingsData.Save();
            }
            EditorGUILayout.HelpBox("* - required field!", MessageType.Info);
            DrawHeader("Shared Resources");
            sharedResourcesView.OnGUI();
            DrawHeader("Scenes");
            scenesView.OnGUI();
            DrawHeader("Packages");
            packagesView.OnGUI();
            DrawHeader("SG Resources");
            sgResourcesView.OnGUI();
        }
        void DrawHeader(string headerName)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(headerName, EditorStyles.boldLabel, GUILayout.Width(160));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

    }
}
