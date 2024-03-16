using SIDGIN.Common.Editors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    internal class SGResourcesBuilder : IBundlesBuilder
    {
        public IEnumerable<BundleData> GetBundles(BuildSettingsData settings)
        {
            var sgResourcesFolders = Directory.GetDirectories(Application.dataPath, "SGResources", SearchOption.AllDirectories);

            List<(string relativePath, BundleData bundle)> bundleDatas = new List<(string relativePath, BundleData)>();
            foreach (var sgResourcesPath in sgResourcesFolders)
            {
                var sgresourcesBundle = bundleDatas.Find(x => x.relativePath == "/");
                if (sgresourcesBundle.bundle == null)
                {
                    sgresourcesBundle = ("/", new BundleData
                    {
                        name = "_sgresources",
                    });

                    var files = GetFiles(sgResourcesPath);
                    if (files != null && files.Any())
                    {
                        sgresourcesBundle.bundle.files.AddRange(files);
                        bundleDatas.Add(sgresourcesBundle);
                    }

                }
                else
                {
                    var files = GetFiles(sgResourcesPath);
                    if (files != null && files.Any())
                        sgresourcesBundle.bundle.files.AddRange(files);
                }

                var subFolders = Directory.GetDirectories(sgResourcesPath);

                foreach (var subFolder in subFolders)
                {
                    AddResourcesForFolder(subFolder, sgResourcesPath, bundleDatas);
                }
            }
            var assetBundleSettings = EditorSettingsLoader.Get<BuildSettingsCacheData>();
            var assetBundlesData = assetBundleSettings.GetSelectedData();
            assetBundlesData.sgResources.Clear();
            foreach (var bundle in bundleDatas)
            {
                assetBundlesData.sgResources.Add(new SGResourceData { bundleName = bundle.bundle.name, localPath = bundle.relativePath });
            }
            assetBundleSettings.Save();
            return bundleDatas.Select(x => x.bundle);
        }
        static void AddResourcesForFolder(string folder, string resourcesPath, List<(string relativePath, BundleData bundle)> bundleDatas)
        {
            var relativePath = folder.Remove(0, resourcesPath.Length);

            var subBundle = bundleDatas.Find(x => x.relativePath == relativePath);
            if (subBundle.bundle == null)
            {
                var bundleName = GetBundleName(relativePath);
                var relativeFiles = GetFiles(folder);
                if (relativeFiles != null && relativeFiles.Any())
                {
                    var sgresourcesBundle = (relativePath, new BundleData
                    {
                        name = bundleName,
                    });
                    sgresourcesBundle.Item2.files = relativeFiles.ToList();
                    bundleDatas.Add(sgresourcesBundle);
                }
            }
            else
            {
                var relativeFiles = GetFiles(folder);
                if (relativeFiles != null && relativeFiles.Any())
                    subBundle.bundle.files.AddRange(relativeFiles);
            }
            var subFolders = Directory.GetDirectories(folder);
            foreach (var subFolder in subFolders)
            {
                AddResourcesForFolder(subFolder, resourcesPath, bundleDatas);
            }


        }
        static string[] GetFiles(string folder)
        {
            return Directory.GetFiles(folder, "*.*")
               .Where(x => !x.Contains(".meta") && !x.Contains(".DS_Store") && !x.Contains(".cs") && !x.Contains(".unity"))
               .Select(x => "Assets" + x.Replace(Application.dataPath, ""))
               .ToArray();
        }
        public static string GetRelativePath(string fullPath)
        {
           var startIndex = fullPath.LastIndexOf("SGResources/");
           return fullPath.Remove(0, startIndex + 11);//SGRessources length + / 
        }
        public static string GetBundleName(string relativePath)
        {
            if (relativePath == "\\" || relativePath == "/")
                return "_sgresources";
            var bundleName = relativePath.Replace("\\", "~").Replace("/", "~");
            var bundleName2 = "";
            foreach (var c in bundleName)
            {
                if (Char.IsUpper(c))
                {
                    bundleName2 += "]";
                }
                bundleName2 += c;
            }
            return "_sgresources" + bundleName2;
        }

    }
}
