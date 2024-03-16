using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
namespace SIDGIN.Patcher.Client
{
    internal static class SGResourcesMananger
    {
        static Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
        internal static void Initialize()
        {
            var sgresourcesPath = Application.persistentDataPath;
            var files = Directory.GetFiles(sgresourcesPath,"*",SearchOption.AllDirectories);
            if (files == null || !files.Any())
                return;
            foreach (var file in files.Where(x => x.Contains("_sgresources") && !x.Contains(".manifest")))
            {
                if (file.Contains("SGPatcherSources"))
                    continue;
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName == "_sgresources")
                {
                    if (!bundles.ContainsKey("/"))
                    {
                        var bundle = AssetBundle.LoadFromFile(file);
                        bundles.Add("/", bundle);
                    }
                }
                else
                {
                    fileName = fileName.Replace("_sgresources~", "");
                    string folderPath = "";
                    bool nextUp = false;
                    foreach (var c in fileName)
                    {
                        if (c == ']')
                        {
                            nextUp = true;
                        }
                        else
                        {
                            if (nextUp)
                            {
                                folderPath += System.Char.ToUpper(c);
                                nextUp = false;
                            }
                            else
                            {
                                folderPath += c;
                            }
                        }
                    }
                    folderPath = folderPath.Replace("~", "/");
                    if (!bundles.ContainsKey(folderPath))
                    {
                        var bundle = AssetBundle.LoadFromFile(file);
                        bundles.Add(folderPath, bundle);

                    }
                }
            }
        }

        internal static void ForceInitialize()
        {
            foreach (var bundle in bundles)
            {
                bundle.Value.Unload(true);
            }
            bundles.Clear();
            Initialize();
        }

        public static AssetBundle GetBundle(string path)
        {
            AssetBundle result = null;
            if (path.Length > 0)
            {
                bundles.TryGetValue(path, out result);
            }
            else
            {
                bundles.TryGetValue("/", out result);
            }
            return result;
        }
        public static IEnumerable<Object> LoadAll(string path)
        {
            bool root = string.IsNullOrEmpty(path) || path == "/";
            foreach (var bundle in bundles)
            {
                if (root || bundle.Key.Contains(path))
                {
                    foreach (var obj in bundle.Value.LoadAllAssets())
                    {
                        yield return obj;
                    }
                }
            }
        }
        public static IEnumerable<Object> LoadAll(string path, System.Type type)
        {
            bool isComponent = type.IsSubclassOf(typeof(Component));
            bool root = string.IsNullOrEmpty(path) || path == "/";
            foreach (var bundle in bundles)
            {
                if (root || bundle.Key.Contains(path))
                {
                    if (isComponent)
                    {
                        var gameObjects = bundle.Value.LoadAllAssets(typeof(GameObject));
                        Object result;
                        foreach(var gameObject in gameObjects)
                        {
                            result = ((GameObject)gameObject).GetComponent(type);
                            if (result != null)
                                yield return result;
                        }
                    }
                    else
                    {
                        foreach (var obj in bundle.Value.LoadAllAssets(type))
                        {
                            yield return obj;
                        }
                    }
                }
            }
        }
        public static IEnumerable<T> LoadAll<T>(string path) where T : Object
        {
            foreach (var obj in LoadAll(path, typeof(T)))
            {
                yield return (T)obj;
            }
        }
    }
}