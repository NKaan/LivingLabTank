using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    public static class EditorSettingsLoader
    {
        private const string MAIN_FOLDER_NAME = "SIDGIN";

        private static Dictionary<System.Type, ScriptableObject>
            cache = new Dictionary<System.Type, ScriptableObject>();

        public static T Get<T>(bool includeInBuild = false) where T : ScriptableObject
        {
            if (EditorSettingsLoader.cache == null)
                EditorSettingsLoader.cache = new Dictionary<System.Type, ScriptableObject>();
            if (!EditorSettingsLoader.cache.ContainsKey(typeof(T)))
                return EditorSettingsLoader.LoadType<T>(includeInBuild);
            if ((UnityEngine.Object) EditorSettingsLoader.cache[typeof(T)] != (UnityEngine.Object) null)
                return EditorSettingsLoader.cache[typeof(T)] as T;
            EditorSettingsLoader.cache.Clear();
            return EditorSettingsLoader.LoadType<T>(includeInBuild);
        }

        private static T LoadType<T>(bool includeInBuild = false) where T : ScriptableObject
        {
            T obj = EditorSettingsLoader.Load(typeof(T), includeInBuild) as T;
            if ((UnityEngine.Object) obj != (UnityEngine.Object) null)
            {
                if (EditorSettingsLoader.cache.ContainsKey(typeof(T)))
                    EditorSettingsLoader.cache[typeof(T)] = (ScriptableObject) obj;
                else
                    EditorSettingsLoader.cache.Add(typeof(T), (ScriptableObject) obj);
            }

            return obj;
        }

        private static ScriptableObject Load(System.Type type, bool includeInBuild = false) =>
            Application.isPlaying & includeInBuild
                ? EditorSettingsLoader._Load(type)
                : EditorSettingsLoader._LoadOrCreate(type, includeInBuild);

        private static ScriptableObject _Load(System.Type type) => Resources.Load(type.Name) as ScriptableObject;

        private static ScriptableObject _LoadOrCreate(System.Type type, bool includeInBuild = false)
        {
            string savePath = EditorSettingsLoader.GetSavePath(type, includeInBuild);
            if (!File.Exists(savePath))
                EditorSettingsLoader.CreateAsset(savePath, type);
            ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath(savePath, type) as ScriptableObject;
            if ((UnityEngine.Object) scriptableObject == (UnityEngine.Object) null)
            {
                EditorSettingsLoader.CreateAsset(savePath, type);
                scriptableObject = AssetDatabase.LoadAssetAtPath(savePath, type) as ScriptableObject;
            }

            return scriptableObject;
        }

        private static void CreateAsset(string pathToAsset, System.Type type) =>
            AssetDatabase.CreateAsset((UnityEngine.Object) ScriptableObject.CreateInstance(type), pathToAsset);

        private static string GetSavePath(System.Type type, bool includeInBuild)
        {
            string path =
                Path.Combine(((IEnumerable<string>) Directory.GetDirectories(Application.dataPath, "SIDGIN",
                        SearchOption.AllDirectories)).FirstOrDefault<string>(),
                    includeInBuild ? "Resources" : "EditorResources");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return Path.Combine(path.Replace(Application.dataPath, "Assets"), type.Name + ".asset");
        }

        public static void Save(this ScriptableObject scriptableObject)
        {
            UnityEditor.EditorUtility.SetDirty(scriptableObject);
        }
    }
}