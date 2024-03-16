using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    internal class SharedResourcesBuilder : IBundlesBuilder
    {
        public IEnumerable<BundleData> GetBundles(BuildSettingsData settings)
        {
            var targetPath = Application.dataPath.Replace("Assets", "");
            foreach (var sharedFolder in settings.sharedFolders)
            {
                yield return new BundleData
                {
                    name = sharedFolder.name,
                    files = GetAllFiles(targetPath + sharedFolder.path)
                };
            }
        }
        static List<string> GetAllFiles(string folder)
        {
            return Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
               .Where(x => !x.Contains(".meta") && !x.Contains(".DS_Store") && !x.Contains(".cs") && !x.Contains(".unity"))
               .Select(x => "Assets" + x.Replace(Application.dataPath, ""))
               .ToList();
        }
    }
}
