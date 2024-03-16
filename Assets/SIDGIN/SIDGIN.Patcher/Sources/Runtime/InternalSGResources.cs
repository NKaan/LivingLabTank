using UnityEngine;
using UnityEngineInternal;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace SIDGIN.Patcher.Client
{
    public class InternalSGResources
    {
        static InternalSGResources()
        {
            SGResourcesMananger.Initialize();
        }
        public static void Initialize()
        {
            SGResourcesMananger.Initialize();
        }

        public static void ForceInitialize()
        {
            SGResourcesMananger.ForceInitialize();
        }
        public static Object[] FindObjectsOfTypeAll(System.Type type)
        {
            var findedObjs = Resources.FindObjectsOfTypeAll(type);
            var result = new List<Object>(findedObjs);
            var objs = LoadAll("", type);
            foreach (var obj in objs)
            {
                if (!result.Contains(obj))
                {
                    result.Add(obj);
                }
            }
            return result.ToArray();
        }
        public static T[] FindObjectsOfTypeAll<T>() where T : Object
        {
            var findedObjs = Resources.FindObjectsOfTypeAll<T>();
            var result = new List<T>(findedObjs);
            var objs = LoadAll<T>("");
            foreach (var obj in objs)
            {
                if (!result.Contains(obj))
                {
                    result.Add(obj);
                }
            }
            return result.ToArray();
        }
        public static Object Load(string path)
        {
            var bundle = SGResourcesMananger.GetBundle(GetDirectoryParent(path));
            if (bundle == null)
                return default;
            return bundle.LoadAsset(Path.GetFileName(path));
        }
        public static T Load<T>(string path) where T : Object
        {
            var bundle = SGResourcesMananger.GetBundle(GetDirectoryParent(path));
            if (bundle == null)
                return default;
            return bundle.LoadAsset<T>(Path.GetFileName(path));
        }

        [TypeInferenceRule(TypeInferenceRules.TypeReferencedBySecondArgument)]
        public static Object Load(string path, [NotNull] System.Type systemTypeInstance)
        {
            var bundle = SGResourcesMananger.GetBundle(GetDirectoryParent(path));
            if (bundle == null)
                return default;
            return bundle.LoadAsset(Path.GetFileName(path), systemTypeInstance);
        }
        public static T[] LoadAll<T>(string path) where T : Object
        {
            return SGResourcesMananger.LoadAll<T>(path).ToArray();
        }

        public static Object[] LoadAll([NotNull] string path, [NotNull] System.Type systemTypeInstance)
        {
            return SGResourcesMananger.LoadAll(path, systemTypeInstance).ToArray();
        }

        public static Object[] LoadAll(string path)
        {
            return SGResourcesMananger.LoadAll(path).ToArray();
        }

        //public static ResourceRequest LoadAsync<T>(string path) where T : Object
        //{

        //}
        //public static ResourceRequest LoadAsync(string path)
        //{

        //}
        //public static ResourceRequest LoadAsync(string path, Type type)
        //{

        //}

        public static void UnloadAssets(string path, bool unloadObjects = false)
        {
            var bundle = SGResourcesMananger.GetBundle(path);
            if (bundle != null)
            {
                bundle.Unload(unloadObjects);
            }
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
                return "";
            }
        }

    }
}
