using System.Linq;
using UnityEditor;
using System.IO;
using SIDGIN.Common.Editors;
using System.Collections.Generic;

namespace SIDGIN.Patcher.Editors
{
    class BundleData
    {
        public string name;
        public List<string> files = new List<string>();
    }
    internal class AssetBundlesBuildHelper
    {

        static IEnumerable<BundleData> PrepareSceneBundlesWithShared(BuildSettingsData settings)
        {
            var builders = new List<IBundlesBuilder>
            {
                new SharedResourcesBuilder(),
                new SceneBuilder(),
            };
            foreach(var builder in builders)
            {
                if (builder is SceneBuilder)
                {
                    foreach (var bundle in ((SceneBuilder)builder).GetBundlesWithShared(settings))
                    {
                        yield return bundle;
                    }
                }
                else
                {
                    foreach (var bundle in builder.GetBundles(settings))
                    {
                        yield return bundle;
                    }
                }
            }
        }

        static IEnumerable<BundleData> PrepareScenes(BuildSettingsData settings)
        {
            var builders = new List<IBundlesBuilder>
            {
                new SceneBuilder(),
            };
            foreach (var builder in builders)
            {
                foreach (var bundle in builder.GetBundles(settings))
                {
                    yield return bundle;
                }
            }
        }
        static IEnumerable<BundleData> PrepareOtherBundles(BuildSettingsData settings)
        {
            var builders = new List<IBundlesBuilder>
            {
                new SGResourcesBuilder(),
                new RuntimeSettingsBuilder()

            };
            foreach (var builder in builders)
            {
                foreach (var bundle in builder.GetBundles(settings))
                {
                    yield return bundle;
                }
            }
        }
        public static void Build(string targetDirectory)
        {
            if (Directory.Exists(targetDirectory))
            {
                Directory.Delete(targetDirectory, true);
            }
            Directory.CreateDirectory(targetDirectory);
            var assetBundleData = EditorSettingsLoader.Get<BuildSettingsCacheData>().GetSelectedData();
            var scenesWithShared = PrepareSceneBundlesWithShared(assetBundleData).ToArray();
            if (scenesWithShared.Length != 0)
            {
                BuildBundles(scenesWithShared, assetBundleData, targetDirectory);
            }
            var scenes = PrepareScenes(assetBundleData).ToArray();
            if (scenes.Length != 0)
            {
                BuildBundles(scenes, assetBundleData, targetDirectory);
            }
            var other = PrepareOtherBundles(assetBundleData).ToArray();
            if(other.Length != 0)
            {
                BuildBundles(other, assetBundleData, targetDirectory);
            }
        }

        static void BuildBundles(BundleData[] bundleDatas, BuildSettingsData settings, string targetDirectory)
        {
            var buildMap = new AssetBundleBuild[bundleDatas.Length];
            for (int i = 0; i < bundleDatas.Length; i++)
            {
                buildMap[i].assetBundleName = bundleDatas[i].name;
                buildMap[i].assetNames = bundleDatas[i].files.ToArray();
            }
            BuildAssetBundleOptions buildCompressOption;
            switch (settings.compressType)
            {
                case BundlesCompressType.LZMA: buildCompressOption = BuildAssetBundleOptions.None; break;
                case BundlesCompressType.Uncompressed: buildCompressOption = BuildAssetBundleOptions.UncompressedAssetBundle; break;
                default: buildCompressOption = BuildAssetBundleOptions.ChunkBasedCompression; break;
            }
            var result = BuildPipeline.BuildAssetBundles(targetDirectory, buildMap, buildCompressOption | BuildAssetBundleOptions.StrictMode, GetBuildTarget(settings.buildTarget));
            if(result == null)
            {
                throw new System.ApplicationException("The patch was canceled because an error occurred while building the build!");
            }
        }
        static BuildTarget GetBuildTarget(InAppBuildTarget source)
        {
            switch (source)
            {
                case InAppBuildTarget.Android: return BuildTarget.Android;
                case InAppBuildTarget.iOS: return BuildTarget.iOS;
                case InAppBuildTarget.PS4: return BuildTarget.PS4;
                case InAppBuildTarget.XboxOne: return BuildTarget.XboxOne;
                case InAppBuildTarget.WSAPlayer:return BuildTarget.WSAPlayer;
                case InAppBuildTarget.Switch:return BuildTarget.Switch;
                case InAppBuildTarget.tvOS:return BuildTarget.tvOS;
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
                case InAppBuildTarget.Lumin: return BuildTarget.Lumin;
#endif
                case InAppBuildTarget.StandaloneWindows: return BuildTarget.StandaloneWindows;
                case InAppBuildTarget.StandaloneWindows64: return BuildTarget.StandaloneWindows64;
                case InAppBuildTarget.StandaloneOSX: return BuildTarget.StandaloneOSX;
                case InAppBuildTarget.StandaloneLinux: return BuildTarget.StandaloneLinux;
                case InAppBuildTarget.StandaloneLinux64: return BuildTarget.StandaloneLinux64;
                case InAppBuildTarget.StandaloneLinuxUniversal: return BuildTarget.StandaloneLinuxUniversal;
                default: return BuildTarget.StandaloneWindows;
            }
        }
    }
}
