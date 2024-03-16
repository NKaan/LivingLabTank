using SIDGIN.Common.Editors;
using System;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor;

namespace SIDGIN.Patcher.Editors
{
    public class PublishManager
    {
        public event Action<float, string> onProcessChanged;
        public async Task Publish()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            if (settingsData.patchTask == null || !settingsData.patchTask.needPatch)
                return;
            var packageBuilder = new PackageBuilder();
            packageBuilder.onProcessChanged += onProcessChanged;
            var tasks = await packageBuilder.BuildPackages();
            var buildSettingsCache = EditorSettingsLoader.Get<BuildSettingsCacheData>();
            var buildSettings = buildSettingsCache.GetSelectedData();

            var packageManagerEditor = new PackageManagerEditor();
            packageManagerEditor.onProcessChanged += onProcessChanged;
            try
            {
                foreach (var uploadTask in tasks)
                {
                    try
                    {
                        await packageManagerEditor.PutPackage(uploadTask);
                        if (buildSettings.packageUploadTasks != null)
                        {
                            buildSettings.packageUploadTasks.RemoveAll(x => x.PackageName == uploadTask.PackageName 
                            && x.Version == uploadTask.Version 
                            && x.VersionFrom == uploadTask.VersionFrom);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        if (buildSettings.packageUploadTasks == null)
                            buildSettings.packageUploadTasks = new System.Collections.Generic.List<PackageUploadTask>();
                        if (!buildSettings.packageUploadTasks.Contains(uploadTask))
                        {
                            var existTask = buildSettings.packageUploadTasks.Find(x => x.PackageName == uploadTask.PackageName 
                            && x.Version == uploadTask.Version 
                            && x.VersionFrom == uploadTask.VersionFrom);
                            if (existTask != null)
                            {
                                existTask.Files = uploadTask.Files;
                                existTask.FullPath = uploadTask.FullPath;
                                existTask.IsFullPackage = uploadTask.IsFullPackage;
                                existTask.PatchPath = uploadTask.PatchPath;
                                existTask.requiredPackages = uploadTask.requiredPackages;
                            }
                            else
                            {
                                buildSettings.packageUploadTasks.Add(uploadTask);
                            }
                        }
                    }

                }
            }
            finally
            {
                buildSettingsCache.Save();
                AssetDatabase.SaveAssets();
                settingsData.patchTask = null;
            }

        }
    }
}
