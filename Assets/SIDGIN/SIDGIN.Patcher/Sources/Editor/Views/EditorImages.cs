using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    public class EditorImages
    {
        static EditorImages _instance;
        static EditorImages instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EditorImages();
                return _instance;
            }
        }
        string resourcesFolder;
        public static string RESOURCES_FOLDER
        {
            get
            {
                if (string.IsNullOrEmpty(instance.resourcesFolder))
                {
                    var ucDir = Directory.GetDirectories(Application.dataPath, EditorConsts.MAIN_FOLDER_NAME, SearchOption.AllDirectories).FirstOrDefault();
                    ucDir = ucDir.Replace(Application.dataPath, "Assets");
                    instance.resourcesFolder = Path.Combine(ucDir, "Editor/Resources");
                }
                return instance.resourcesFolder;
            }
        }

        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public static Texture2D Get(string key)
        {
            if (instance.textures.ContainsKey(key))
            {
                return instance.textures[key]; 
            }
            var pathTexture = Path.Combine(RESOURCES_FOLDER, $"{key}.png");
            var texture =  AssetDatabase.LoadAssetAtPath<Texture2D>(pathTexture);
            if(texture == null)
            {
                throw new System.ApplicationException("Editor image not found!!!");
            }
            instance.textures.Add(key, texture);
            return texture;
        }
    }
}
