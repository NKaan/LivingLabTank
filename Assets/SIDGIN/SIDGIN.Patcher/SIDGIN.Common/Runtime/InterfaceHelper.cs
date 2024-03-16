using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace SIDGIN.Common
{
    public static class InterfaceHelper
    {

        public static IList<T> FindObjects<T>() where T : class
        {
            var objects = new List<T>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    var objs = root.GetComponentsInChildren<T>(true);
                    if (objs != null)
                        objects.AddRange(objs);
                }
            }
            return objects;
        }

        public static T FindObject<T>() where T : class
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    var obj = root.GetComponentInChildren<T>(true);
                    if (obj != null)
                        return obj;
                }
            }
            return null;
        }
        public static IEnumerable<Type> GetChildTypes<T>() where T : class
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in assembly.GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                {
                    yield return type;
                }
        }
        public static IEnumerable<Type> GetChildTypesByInterface<T>() 
        {
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type type in assembly.GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && typeof(T).IsAssignableFrom(myType)))
            {
                yield return type;
            }
        }
    }
}