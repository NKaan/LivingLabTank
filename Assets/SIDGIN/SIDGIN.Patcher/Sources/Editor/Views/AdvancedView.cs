using SIDGIN.Common.Editors;
using SIDGIN.Patcher.Client;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    public class AdvancedView
    {
        public void OnEnable()
        {

        }
        public void OnDraw()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            if (GUILayout.Button("Open Cache"))
            {
                OpenCacheWindow();
            }
            if (GUILayout.Button("Clear Cache"))
            {
                EditorDispatcher.Dispatch(ClearCache);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Default Language Sheet");
            if(GUILayout.Button("Save File (English)"))
            {
                var fileDialog = EditorUtility.SaveFilePanel("Save localization file", Application.dataPath, "English", "xml");
                if (!string.IsNullOrEmpty(fileDialog))
                {
                    var localizeSheet = new LocalizeSheet
                    {
                        localizeKeys = LocalizationDefault.Keys.Select(x =>
                        new LocalizeSheet.LocalizeKey { Key = x.Key, Value = x.Value }).ToList()
                    };
                    LocalizeSheet.Save(fileDialog, LocalizationDefault.Keys);
                }
            }
            EditorGUILayout.EndHorizontal();

            var settingsData = EditorSettingsLoader.Get<SettingsData>();

            GUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Compress Level:", GUILayout.Width(140));
            settingsData.zipCompressLevel = EditorGUILayout.IntSlider(settingsData.zipCompressLevel, 0, 9);
            GUILayout.EndHorizontal();

           
        }
        void ClearCache()
        {
            try
            {
                System.IO.Directory.Delete(EditorConsts.TEMP_PATH, true);
                System.IO.Directory.Delete(EditorConsts.VERSIONS_PATH, true);
                EditorUtility.DisplayDialog("Clear cache", "Cache flushed successfully.", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Problem with clearing the cache, do it manually. " + ex);
            }

        }
        void OpenCacheWindow()
        {
            EditorDispatcher.Dispatch(() => EditorUtility.RevealInFinder(EditorConsts.CACHE_PATH));
        }
    }
}
