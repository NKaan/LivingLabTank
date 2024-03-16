
using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngineInternal;

namespace SIDGIN.Patcher.Editors
{
    public class InternalSGResourcesEditor
    {
        static Dictionary<string, List<string>> dirMap;
        static string projectPath;
        
        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            LoadDirMap();
            projectPath = Application.dataPath.Remove(Application.dataPath.Length - 6, 6);
        }

        public static void ForceInitialize()
        {
            Initialize();
        }
        static InternalSGResourcesEditor()
        {
            Initialize();
        }
        static void LoadDirMap()
        {
            dirMap = new Dictionary<string, List<string>>();
            var sgResourcesFolders = Directory.GetDirectories(Application.dataPath, "SGResources", SearchOption.AllDirectories);

            foreach (var folder in sgResourcesFolders)
            {
                var assetPath = folder.Remove(0, Application.dataPath.Length - 6).Replace("\\", "/");
                if (dirMap.ContainsKey("/"))
                {
                    dirMap["/"].Add(assetPath);
                }
                else
                {
                    dirMap.Add("/", new List<string> { assetPath });
                }
                var childFolders = Directory.GetDirectories(folder, "*", SearchOption.AllDirectories);
                foreach (var childFolder in childFolders)
                {
                    var childAssetPath = childFolder.Remove(0, Application.dataPath.Length - 6).Replace("\\", "/");
                    var relativePath = childFolder.Remove(0, folder.Length + 1).Replace("\\", "/");
                    if (dirMap.ContainsKey(relativePath))
                    {
                        if (!dirMap[relativePath].Contains(childAssetPath))
                        {
                            dirMap[relativePath].Add(childAssetPath);
                        }
                    }
                    else
                    {
                        dirMap.Add(relativePath, new List<string> { childAssetPath });
                    }

                }
            }
        }


        public static Object[] FindObjectsOfTypeAll(System.Type type)
        {
            return Resources.FindObjectsOfTypeAll(type);
        }

        public static T[] FindObjectsOfTypeAll<T>() where T : Object
        {
            return Resources.FindObjectsOfTypeAll<T>();
        }

        public static Object Load(string path)
        {
            var fileName = System.IO.Path.GetFileName(path);
            foreach (var assetPath in GetListDirectories(path, true))
            {
                if (!Directory.Exists(projectPath + assetPath))
                {
                    continue;
                }
                foreach (var file in GetFiles(assetPath, fileName))
                {
                    var loadPath = file.Remove(0, projectPath.Length);
                    return AssetDatabase.LoadAssetAtPath(loadPath, typeof(Object));
                }
            }
            return null;
        }
        public static T Load<T>(string path) where T : Object
        {
            var fileName = System.IO.Path.GetFileName(path);
            foreach (var assetPath in GetListDirectories(path, true))
            {
                if (!Directory.Exists(projectPath + assetPath))
                {
                    continue;
                }
                foreach (var file in GetFiles(assetPath, fileName))
                {
                    var loadPath = file.Remove(0, projectPath.Length);
                    return AssetDatabase.LoadAssetAtPath<T>(loadPath);
                }
            }
            return null;
        }

        [TypeInferenceRule(TypeInferenceRules.TypeReferencedBySecondArgument)]
        public static Object Load(string path, [NotNull] System.Type systemTypeInstance)
        {
            var fileName = System.IO.Path.GetFileName(path);
            foreach (var assetPath in GetListDirectories(path, true))
            {
                if (!Directory.Exists(projectPath + assetPath))
                {
                    continue;
                }
                foreach (var file in GetFiles(assetPath, fileName))
                {
                    var loadPath = file.Remove(0, projectPath.Length);
                    return AssetDatabase.LoadAssetAtPath(loadPath, systemTypeInstance);
                }
            }
            return null;
        }

        public static IEnumerable<T> LoadAllEnumerable<T>(string path) where T : Object
        {
            foreach (var assetPath in GetListDirectories(path, false))
            {
                if (!Directory.Exists(projectPath + assetPath))
                {
                    continue;
                }
                foreach (var file in GetFilesInAll(assetPath))
                {
                    var loadPath = file.Remove(0, projectPath.Length);
                    var asset = AssetDatabase.LoadAssetAtPath<T>(loadPath);
                    if (asset != null)
                        yield return asset;
                }
            }
        }
        public static IEnumerable<Object> LoadAllEnumerable(string path, [NotNull] System.Type systemTypeInstance)
        {
            foreach (var assetPath in GetListDirectories(path, false))
            {
                if (!Directory.Exists(projectPath + assetPath))
                {
                    continue;
                }
                foreach (var file in GetFilesInAll(assetPath))
                {
                    var loadPath = file.Remove(0, projectPath.Length);
                    var asset = AssetDatabase.LoadAssetAtPath(loadPath, systemTypeInstance);
                    if (asset != null)
                        yield return asset;

                }
            }
        }
        public static T[] LoadAll<T>(string path) where T : Object
        {
            return LoadAllEnumerable<T>(path).ToArray();
        }

        public static Object[] LoadAll([NotNull] string path, [NotNull] System.Type systemTypeInstance)
        {
            return LoadAllEnumerable(path, systemTypeInstance).ToArray();
        }

        public static Object[] LoadAll(string path)
        {
            return LoadAllEnumerable(path, typeof(Object)).ToArray();
        }
        public static void UnloadAssets(string path, bool unloadObjects = false)
        {

        }

        static IEnumerable<string> GetListDirectories(string path, bool forParent)
        {
            string parent = path;
            if (forParent)
            {
                parent = GetDirectoryParent(path);
            }
            else if (string.IsNullOrEmpty(path))
            {
                parent = "/";
            }

            if (dirMap.TryGetValue(parent, out List<string> paths))
            {
                foreach (var assetPath in paths)
                {
                    yield return assetPath;
                }
            }
        }
        static IEnumerable<string> GetFiles(string assetPath, string fileName = "")
        {
            return Directory.GetFiles(projectPath + assetPath, $"{fileName}*").Where(x => !x.Contains(".meta"));
        }
        static IEnumerable<string> GetFilesInAll(string assetPath, string fileName = "")
        {
            return Directory.GetFiles(projectPath + assetPath, "*", SearchOption.AllDirectories).Where(x => !x.Contains(".meta"));
        }
        static string GetDirectoryParent(string sourcePath)
        {
            int indexOf = sourcePath.LastIndexOf('/');
            if (indexOf != -1)
            {
                return sourcePath.Remove(indexOf, sourcePath.Length - indexOf);
            }
            else
            {
                return "/";
            }
        }

    }
}
