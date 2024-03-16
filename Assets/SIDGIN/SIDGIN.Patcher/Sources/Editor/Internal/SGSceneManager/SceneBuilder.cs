using SIDGIN.Patcher.Client;
using System.Collections.Generic;
namespace SIDGIN.Patcher.Editors
{
    internal class SceneBuilder : IBundlesBuilder
    {
        public IEnumerable<BundleData> GetBundlesWithShared(BuildSettingsData settings)
        {
            foreach (var scene in settings.scenes)
            {
                if (!string.IsNullOrEmpty(scene.sharedResourcesKey) && scene.sharedResourcesKey != "None")
                {
                    yield return new BundleData()
                    {
                        name = $"{Consts.SCENE_PREFIX}{scene.index}",
                        files = new List<string> { scene.path }
                    };
                }
            }
        }
        public IEnumerable<BundleData> GetBundles(BuildSettingsData settings)
        {
            foreach (var scene in settings.scenes)
            {
                if (string.IsNullOrEmpty(scene.sharedResourcesKey) || scene.sharedResourcesKey == "None")
                {
                    yield return new BundleData()
                    {
                        name = $"{Consts.SCENE_PREFIX}{scene.index}",
                        files = new List<string> { scene.path }
                    };
                }
            }
        }
    }
}
