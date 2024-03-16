using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SIDGIN.Patcher.Editors
{
    using SIDGIN.Common.Editors;
    using SIDGIN.Patcher.Editors.Storages;

    public class SettingsView
    {
        DefinitionsView defView = new DefinitionsView();
        string[] modules = new string[] 
        { 
#if SGPATCHER_GOOGLE
            "Google",
#endif
            "Http", 
            "AmazonS3" 
        };
        int moduleIndex;
        StorageSettingsView storageSettingsView;
        AssetBundlesSettingsView assetBundlesView = new AssetBundlesSettingsView();
        Vector2 scrollVector;
        public void OnEnable()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            moduleIndex = modules.ToList().IndexOf(settingsData.selectedStorageModule);
            SetupStorageSettingsView();
            defView.OnEnable();
            assetBundlesView.OnEnable();
        }
        public void OnDraw()
        {
            scrollVector = EditorGUILayout.BeginScrollView(scrollVector);
            DrawStorageTypeSelector();
            DrawStorageSettings();
            defView.OnDraw();
            DrawBuildSettings();
            EditorGUILayout.EndScrollView();
        }
        void SetupStorageSettingsView()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            switch (settingsData.selectedStorageModule)
            {
#if SGPATCHER_GOOGLE
                case "Google": storageSettingsView = new GoogleSettingsView(); break;
#endif
                case "Http": storageSettingsView = new HttpSettingsView(); break;
                case "AmazonS3": storageSettingsView = new AmazonSettingsView(); break;
            }

        }
        void DrawStorageSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Storage Settings", EditorStyles.boldLabel,GUILayout.Width(160));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (storageSettingsView != null)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                storageSettingsView.OnDraw();
                EditorGUILayout.EndVertical();
            }
        }
        void DrawBuildSettings()
        {

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel, GUILayout.Width(160));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (assetBundlesView != null)
            {
                assetBundlesView.OnDraw();
            }
        }
        void DrawStorageTypeSelector()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Storage API:");
            EditorGUI.BeginChangeCheck();
            moduleIndex = EditorGUILayout.Popup(moduleIndex, modules);
            if (EditorGUI.EndChangeCheck())
            {
                settingsData.selectedStorageModule = modules[moduleIndex];
                settingsData.SelectDefaultDefinition();
                settingsData.Save();
                ClientSettingsSaver.SaveClientSettings();
                SetupStorageSettingsView();
            }
            EditorGUILayout.EndHorizontal();
         
        }



    }
}