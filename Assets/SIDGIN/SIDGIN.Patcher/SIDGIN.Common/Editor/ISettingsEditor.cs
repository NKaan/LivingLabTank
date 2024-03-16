using System.IO;
using System.Linq;
using UnityEngine;

namespace SIDGIN.Common.Editors
{
    public abstract class ISettingsEditor<T> : ScriptableObject where T : ISettingsEditor<T>
    {
        const string MAIN_FOLDER_NAME = "SIDGIN";

        static T settings;
        public static T Settings
        {
            get
            {
                if (settings != null)
                    return settings;
                if (Application.isPlaying)
                {
                    settings = Load();
                    return settings;
                }
                else
                {
                    settings = LoadOrCreate();
                    return settings;
                }
            }
        }
        static T Load()
        {
            return Resources.Load<T>(typeof(T).Name);
        }

        static T LoadOrCreate()
        {
            var pathToAsset = GetSavePath();
            if (!File.Exists(pathToAsset))
            {
                CreateAsset(pathToAsset);
            }
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(pathToAsset);
            if (asset == null)
            {
                CreateAsset(pathToAsset);
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(pathToAsset);
            }
            return asset;
        }


        static void CreateAsset(string pathToAsset)
        {
            var settings = CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(settings, pathToAsset);
        }
        static string GetSavePath()
        {
            var dir = Directory.GetDirectories(Application.dataPath, MAIN_FOLDER_NAME, SearchOption.AllDirectories).FirstOrDefault();
            var resourcesDir = Path.Combine(dir, "Resources");
            if (!Directory.Exists(resourcesDir))
            {
                Directory.CreateDirectory(resourcesDir);
            }
            resourcesDir = resourcesDir.Replace(Application.dataPath, "Assets");
            var ucSettingsAssetPath = Path.Combine(resourcesDir, typeof(T).Name + ".asset");
            return ucSettingsAssetPath;
        }

        public void Save()
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
}
