using System.Collections.Generic;
using System.Linq;
using System.IO;
using SIDGIN.Common.Editors;
using SIDGIN.Patcher.Common;
using System.Threading.Tasks;
using SIDGIN.Zip;
using System;
using SIDGIN.Patcher.Delta;
using SIDGIN.Patcher.Client;
using UnityEngine;
using Version = SIDGIN.Patcher.Client.Version;

namespace SIDGIN.Patcher.Editors
{
    public class PackageMeta
    {
        public string name;
        public List<string> requiredPackages = new List<string>();
        public bool isFullPackage;
        public List<SceneEditorData> scenes = new List<SceneEditorData>();
        public List<SharedFolderData> sharedResources = new List<SharedFolderData>();

    }
    public class PackageBuilder
    {

        public event Action<float, string> onProcessChanged;
        public List<PackageMeta> PreparePackages(BuildSettingsData bundleData)
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            var result = new List<PackageMeta>();


            foreach (var sh_item in bundleData.sharedFolders)
            {
                result.Add(new PackageMeta
                {
                    name = $"{sh_item.name}_package",
                    sharedResources = new List<SharedFolderData> { sh_item },
                    isFullPackage = settingsData.patchTask.isInitial
                });

            }
            foreach (var packageName in bundleData.packages)
            {
                result.Add(new PackageMeta
                {
                    name = packageName,
                    isFullPackage = settingsData.patchTask.isInitial
                });
            }
            foreach (var scene in bundleData.scenes)
            {
                var package = result.Find(x => x.name == scene.packageName);
                if (package != null)
                {
                    package.scenes.Add(scene);
                    if (!string.IsNullOrEmpty(scene.sharedResourcesKey) && scene.sharedResourcesKey != "None")
                    {
                        var sharedId = $"{scene.sharedResourcesKey}_package";
                        if (!package.requiredPackages.Contains(sharedId))
                        {
                            package.requiredPackages.Add(sharedId);
                        }
                    }
                }

            }
            return result;
        }

        public Task<List<PackageUploadTask>> BuildPackages()
        {
            var packageManager = new PackageManagerEditor();
            var assetBundlesData = EditorSettingsLoader.Get<BuildSettingsCacheData>().GetSelectedData();
            var packages = PreparePackages(assetBundlesData);
            return BuildPackages(packages, packageManager);
        }
        public async Task<List<PackageUploadTask>> BuildPackages(List<PackageMeta> packages, PackageManagerEditor packageManager)
        {
            var result = new List<PackageUploadTask>();
            var lastPackagesVersion = await packageManager.GetLastPackagesVersion();
            await CheckInstalledPreviousVersion(lastPackagesVersion);
            foreach (var package in packages)
            {
                var task = await BuildPackage(package,lastPackagesVersion, packageManager);
                if (task != null)
                {
                    result.Add(task);
                }
            }

            RemoveCache();
            return result;
        }

        static string GetTempFile()
        {
            return Path.Combine(EditorConsts.TEMP_PATH, $"{System.Guid.NewGuid().ToString()}.tmp");
        }
        async Task CheckInstalledPreviousVersion(Version version)
        {
            if(version  == Version.Empty)
                return;
            
            var versionStr = version.ToString();
            var previousDirectory = Path.Combine(EditorConsts.VERSIONS_PATH, versionStr);
            //check and install previousPackage
            if (Directory.Exists(previousDirectory))
            {
                return;
            }
            var patcherClient = new PatcherClient(previousDirectory,false);
            patcherClient.onProgressChanged += progress => onProcessChanged?.Invoke(progress.progress,progress.status);
            await patcherClient.Update(version);
            await patcherClient.InstallAllPackages(version);
        }
        void RemoveCache()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            if (!string.IsNullOrEmpty(settingsData.SelectedDefinition.toDeleteFromCacheVersion))
            {
                var toDeleteDirectory = Path.Combine(EditorConsts.VERSIONS_PATH,
                    settingsData.SelectedDefinition.toDeleteFromCacheVersion);
                if (Directory.Exists(toDeleteDirectory))
                {
                    try
                    {  
                        DirectoryHelper.DeleteDirectory(toDeleteDirectory);
                    }
                    catch
                    {
                    }
                }

                settingsData.SelectedDefinition.toDeleteFromCacheVersion = "";
                settingsData.Save();
            }
        }
        public async Task<PackageUploadTask> BuildPackage(PackageMeta package, Version preVersion, PackageManagerEditor packageManager)
        {
            var existPackage = await packageManager.GetPackage(package.name);
            var files = new List<string>();
            var assetBundlesData = EditorSettingsLoader.Get<BuildSettingsCacheData>().GetSelectedData();
            if (package.name == Consts.MAIN_PACKAGE_NAME)
            {
                files.Add(Consts.RUNTIME_DATA_NAME);
                var sgResourceFiles = assetBundlesData.sgResources.Where(x =>
                    !assetBundlesData.sgResourcePackage.Any(y => y.packageName != package.name && y.bundleName == x.bundleName));
                foreach (var sgResourceData in sgResourceFiles)
                {
                    if (!files.Contains(sgResourceData.bundleName))
                    {
                        files.Add(sgResourceData.bundleName);
                    }
                }

            }
            else
            {
                var sgResourceFiles = assetBundlesData.sgResourcePackage.Where(x => x.packageName == package.name);
                foreach (var sgResourceData in sgResourceFiles)
                {
                    if (!files.Contains(sgResourceData.bundleName))
                    {
                        files.Add(sgResourceData.bundleName);
                    }
                }
            }
            foreach (var scene in package.scenes)
            {
                files.Add($"{Consts.SCENE_PREFIX}{scene.index}");
            }
            foreach (var sharedResources in package.sharedResources)
            {
                files.Add($"{sharedResources.name}");
            }
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            
            var currentDirectory = Path.Combine(EditorConsts.VERSIONS_PATH, settingsData.patchTask.version);
     
            var uploadTask = new PackageUploadTask()
            {
                PackageName = package.name,
                Version = settingsData.patchTask.version,
                MainVersion = Application.version,
                requiredPackages = package.requiredPackages
            };
            bool changed = false;
            if (existPackage != null)
            {
                var lastPackageData = existPackage.Patches.LastOrDefault();
                var previousDirectory = Path.Combine(EditorConsts.VERSIONS_PATH, preVersion.ToString());
                var fileEntries = new List<FileEntry>();
                foreach (var file in lastPackageData.Files)
                {
                    if (!files.Contains(file.Filepath))
                    {
                        changed = true;
                        fileEntries.Add(new FileEntry
                        {
                            Filename = file.Filepath,
                            Type = EntryType.Removed,
                            HashOld = HashHelper.GetHashFromFile(Path.Combine(previousDirectory, file.Filepath))
                        });
                    }
                }
                foreach (var file in files)
                {
                    if (!lastPackageData.Files.Select(x => x.Filepath).Contains(file))
                    {
                        changed = true;
                        fileEntries.Add(new FileEntry
                        {
                            Filename = file,
                            Type = EntryType.Added,
                            HashNew = HashHelper.GetHashFromFile(Path.Combine(currentDirectory, file))
                        });
                    }
                    else
                    {
                        var file1 = Path.Combine(previousDirectory, file);
                        var file2 = Path.Combine(currentDirectory, file);
                        if (HashHelper.GetHashFromFile(file1) != HashHelper.GetHashFromFile(file2))
                        {
                            changed = true;
                            fileEntries.Add(new FileEntry
                            {
                                Filename = file,
                                Type = EntryType.Modified,
                                HashOld = HashHelper.GetHashFromFile(file1),
                                HashNew = HashHelper.GetHashFromFile(file2),
                            });
                        }

                    }
                }

                if (changed)
                {
                    var patchFile = GetTempFile();
                    onProcessChanged?.Invoke(0, "Creating a patch...");
                    using (FileStream fsOut = File.Create(patchFile))
                    {
                        using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
                        {
                            zipStream.SetLevel(settingsData.zipCompressLevel);

                            for (int i = 0; i < fileEntries.Count; i++)
                            {
                                var entry = fileEntries[i];
                                MakePatchForFile(entry, zipStream, previousDirectory, currentDirectory,
                                    (p, status) => onProcessChanged?.Invoke((float)(i - 1 + p) / fileEntries.Count, status));

                            }
                            var patchEntry = new PatchEntry { BlockSize = PatchHelper.BLOCK_SIZE, FileEntries = fileEntries };
                            CompressHelper.CompressFileFromData(zipStream, "configuration.json", patchEntry.ToXML());

                            zipStream.IsStreamOwner = true;
                            zipStream.Close();
                        }
                    }

                    onProcessChanged?.Invoke(1, "Patch creation complete.");
                    uploadTask.PatchPath = patchFile;
                    uploadTask.VersionFrom = lastPackageData.Version.ToString();
                    if (string.IsNullOrEmpty(settingsData.SelectedDefinition.toDeleteFromCacheVersion) ||
                        settingsData.SelectedDefinition.toDeleteFromCacheVersion.ToVersion() < preVersion)
                    {
                        settingsData.SelectedDefinition.toDeleteFromCacheVersion = preVersion.ToString();
                        settingsData.Save();
                    }
                }

            }


            if (package.isFullPackage || existPackage == null)
            {

                var outputFile = GetTempFile();
                CompressHelper.CompressFiles(files, outputFile, null);
                var versionDirectory = Path.Combine(EditorConsts.VERSIONS_PATH, settingsData.patchTask.version);
                uploadTask.IsFullPackage = true;
                uploadTask.FullPath = outputFile;
                if (existPackage != null)
                {
                    var fullPatch = existPackage.Patches.FirstOrDefault(x => x.IsInitial);
                    if (fullPatch != null)
                    {
                        fullPatch.PackageName = package.name;
                        await packageManager.DeleteFullPatch(fullPatch);
                    }
                }
            }
            if (!changed && !(package.isFullPackage || existPackage == null))
            {
                return null;
            }

            uploadTask.Files = files.Select(file => new HashFileInfo
            {
                Filepath = file,
                Hash = HashHelper.GetHashFromFile(Path.Combine(currentDirectory, file))
            }).ToList();

            return uploadTask;
        }
        
        void MakePatchForFile(FileEntry entry, ZipOutputStream zipStream, string folder1, string folder2, Action<float, string> callback)
        {

            if (entry.Type == EntryType.Added)
            {
                CompressHelper.CompressFile(zipStream, Path.Combine(folder2, entry.Filename), entry.Filename, p =>
                {
                    callback?.Invoke(p, $"File compression {entry.Filename}");
                });
            }

            if (entry.Type == EntryType.Modified)
            {
                callback?.Invoke(0, $"Creating a patch for {entry.Filename}");
                var tempName = Path.GetTempFileName();
                PatchHelper.MakeDifferenceForFile(Path.Combine(folder1, entry.Filename), Path.Combine(folder2, entry.Filename), tempName);

                CompressHelper.CompressFile(zipStream, tempName, entry.Filename, p =>
                {
                    callback?.Invoke(p, $"File compression {entry.Filename}");
                });
                callback?.Invoke(1f, $"File compression {entry.Filename}");
                File.Delete(tempName);
            }
        }

        


    }
}