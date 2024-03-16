
using UnityEditor;
using UnityEngine;
using System.IO;
namespace SIDGIN.Patcher.Unity
{
    public class SGPatcherAssetPostprocessor : AssetPostprocessor
    {
		const string PACKNAME = "SIDGIN.Patcher";

		static void OnPostprocessAllAssets(
		string[] importedAssets,
		string[] deletedAssets,
		string[] movedAssets,
		string[] movedFromAssetPaths)
		{
			foreach (var str in importedAssets)
			{
				if (str.Contains(PACKNAME))
				{
					SGPatcherInstaller.Install();
					break;
				}
			}
		}
	}
	public static class SGPatcherInstaller
	{
		public static void Install()
		{
			string resourcesPath = Path.Combine(Application.dataPath, "SIDGIN","Resources");
			string editorResourcesPath = Path.Combine(Application.dataPath, "SIDGIN","EditorResources");
			string assetBundlesFile = Path.Combine(resourcesPath, "AssetBundlesData.asset");
			string[] moveFiles = new string[]
			{
				"AssetBundlesData.asset",
				"GoogleSettingsData.asset",
				"HttpSettingsData.asset",
				"SettingsData.asset"
			};
			foreach(var moveFile in moveFiles)
			{
				var resFile = Path.Combine(resourcesPath, moveFile);
				if (File.Exists(resFile))
				{
					try
					{
						if (!Directory.Exists(editorResourcesPath))
							Directory.CreateDirectory(editorResourcesPath);
						File.Move(resFile, Path.Combine(editorResourcesPath, moveFile));
					}
					catch { }
				}
			}
		}
	}
}