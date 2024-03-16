using SIDGIN.Common.Editors;
using SIDGIN.Patcher.Client;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace SIDGIN.Patcher.Editors
{
    internal class RuntimeSettingsBuilder : IBundlesBuilder
    {
        public IEnumerable<BundleData> GetBundles(BuildSettingsData settings)
        {
            var runtimeData = EditorSettingsLoader.Get<RuntimeData>();
            runtimeData.sceneDatas = settings.scenes.Select(scene => new SceneData
            {
                index = scene.index,
                name = scene.name,
                sharedResources = scene.sharedResourcesKey
            }).ToList();
            runtimeData.sharedResources = settings.sharedFolders.Select(shared => shared.name).ToList();
            runtimeData.Save();
            AssetDatabase.SaveAssets();
            yield return new BundleData
            {
                name = Consts.RUNTIME_DATA_NAME,
                files = new List<string> { AssetDatabase.GetAssetPath(runtimeData) }
            };
        }
    }
}
