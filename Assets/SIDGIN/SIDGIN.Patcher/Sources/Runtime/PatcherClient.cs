using System;
using System.Threading.Tasks;
namespace SIDGIN.Patcher.Client
{
    using Common;
    using Storages;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using System.Text;
    using SIDGIN.Patcher.Delta;

    public class PackageTask
    {
        public enum TaskType
        {
            UPDATE,
            INSTALL
        }
        public string name;
        public string version;
        public TaskType taskType;
        public bool updateMainBuildRequired;
        public List<PatchMeta> patches;
        public List<string> requiredPackages;
    }
    public class PatcherClient
    {
        public event Action<PatcherProgress> onProgressChanged;
        private bool needFolderForEachPackage = true;
        float deltaProgress;
        float mainProgress = 0;
        private string targetDirectory;
        StorageApi storage;
        public PatcherClient()
        {
            this.targetDirectory = Application.persistentDataPath;
            storage = Storage.Get();
            storage.onProgressChanged += Storage_onProgressChanged;
        }

        public PatcherClient(string targetDirectory, bool needFolderForEachPackage)
        { 
            this.targetDirectory = targetDirectory;
            this.needFolderForEachPackage = needFolderForEachPackage;
            storage = Storage.Get();
            storage.onProgressChanged += Storage_onProgressChanged;
        }
        void Storage_onProgressChanged(ProgressData data)
        {
            var downloadedSize = data.downloadedBytes.NormalizeFileSize();
            var targetSize = data.targetBytes.NormalizeFileSize();
            var speed = $"{data.bytesPerSecond.NormalizeFileSize()}/s";
            var status = Localize.Format("Downloading_file", $"{downloadedSize}/{targetSize}", speed);
            onProgressChanged?.Invoke(new PatcherProgress
            {
                status = status,
                progress = mainProgress + data.progress * deltaProgress,
                downloadProgress = new DownloadProgress
                {
                    progress = data.progress,
                    status = status,
                    speed = speed,
                    downloadedSize = downloadedSize,
                    targetSize = targetSize,

                }
            });
        }

        async Task<ApplicationRemoteData> GetApplicationRemoteData()
        {
            Debug.Log("[SG Patcher] - Getting application remote data...");
            var settings = ClientSettings.Get();
            var applicationDataStr = await storage.DownloadData(settings.appId);
            var applicationData = XmlSerializeHelper.Deserialize<ApplicationRemoteData>(applicationDataStr);
            Debug.Log("[SG Patcher] - Application remote data received...");
            return applicationData;
        }
        public ApplicationData GetApplicationData()
        {
            var applicationDataPath = Path.Combine(targetDirectory, Consts.APP_DATA_FILE_NAME);
            if (File.Exists(applicationDataPath))
            {
                try
                {
                    var applicationDataStr = File.ReadAllText(applicationDataPath);
                    var applicationData = XmlSerializeHelper.Deserialize<ApplicationData>(applicationDataStr);

                    if (applicationData != null)
                    {
                        return applicationData;
                    }
                    else
                    {
                        File.Delete(applicationDataPath);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    //Ошибка чтения
                    Debug.LogError($"[SG Patcher] - Error reading {Consts.APP_DATA_FILE_NAME} file: " + ex);
                    File.Delete(applicationDataPath);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        void SaveApplicationData(ApplicationData data)
        {
            Debug.Log($"[SG Patcher] - Save application data...");
            var applicationDataPath = Path.Combine(targetDirectory, Consts.APP_DATA_FILE_NAME);
            var dataStr = XmlSerializeHelper.Serialize(data);
            File.WriteAllText(applicationDataPath, dataStr);
        }
        PackageTask GetPackageTask(PackageInfo packageInfo, ApplicationRemoteData remoteData, Version targetVersion)
        {
            var remotePackageData = remoteData.Packages.Find(x => x.Name == packageInfo.name);
            if (remotePackageData != null)
            {
                remotePackageData.Patches.Sort();
                remotePackageData.Patches.Reverse();
                
                var lastPatchToAll = remotePackageData.Patches.LastOrDefault();
                PatchMeta lastPatch = null;
                if (targetVersion == Version.Empty)
                {
                    lastPatch = remotePackageData.Patches.LastOrDefault(x => x.MainVersion == Application.version);
                }
                else
                {
                    lastPatch = remotePackageData.Patches.LastOrDefault(x => x.Version <= targetVersion);
                }

                bool updateMainBuildRequired = lastPatch != lastPatchToAll;
                if (lastPatch == null)
                {
                    return new PackageTask
                    {
                        name = packageInfo.name,
                        updateMainBuildRequired = updateMainBuildRequired,
                        version = Version.Empty.ToString(),
                        patches = new List<PatchMeta>(),
                    };
                }
                if (lastPatch.Version != packageInfo.Version)
                {
                    var packageIndex = remotePackageData.Patches.FindLastIndex(x => x.Version == packageInfo.Version);
                    if (packageIndex != -1 && packageIndex + 1 < remotePackageData.Patches.Count)
                    {
                        var patches = remotePackageData.Patches.GetRange(packageIndex + 1, remotePackageData.Patches.Count - packageIndex - 1);

                        return new PackageTask
                        {
                            name = packageInfo.name,
                            updateMainBuildRequired = updateMainBuildRequired,
                            version = lastPatch.VersionStr,
                            patches = patches,
                            taskType = PackageTask.TaskType.UPDATE,
                            requiredPackages = remotePackageData.RequiredPackages,
                        };
                    }
                    else
                    {
                        var initialIndex = remotePackageData.Patches.FindLastIndex(x => x.IsInitial);
                        if (initialIndex != -1)
                        {
                            var patches = remotePackageData.Patches.GetRange(initialIndex, remotePackageData.Patches.Count - initialIndex);
                            return new PackageTask
                            {
                                name = packageInfo.name,
                                updateMainBuildRequired = updateMainBuildRequired,
                                version = lastPatch.VersionStr,
                                patches = patches,
                                taskType = PackageTask.TaskType.INSTALL,
                                requiredPackages = remotePackageData.RequiredPackages,
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Task<UpdateMetaData> GetUpdateMetaData()
        {
            return GetUpdateMetaData(Version.Empty);
        }
        public async Task<UpdateMetaData> GetUpdateMetaData(Version targetVersion)
        {
            var result = new UpdateMetaData();
            var applicationRemoteData = await GetApplicationRemoteData();
            Debug.Log("[SG Patcher] - Getting local application data...");
            var tasks = GetPackageTasks(applicationRemoteData, targetVersion);
            if (tasks == null || tasks.Count == 0)
            {
                result.updateStatus = UpdateStatus.No_Required;
                return result;
            }

            long updateSize = 0;
            int patchCount = 0;
            result.updateStatus = UpdateStatus.Required;
            List<Version> updateVersions = new List<Version>();
            foreach (var task in tasks)
            {
                foreach (var patch in task.patches)
                {
                    if (patch.IsInitial)
                    {
                        updateSize += patch.FullSize;
                    }
                    else
                    {
                        updateSize += patch.PatchSize;
                    }

                    var patchVersion = patch.Version;
                    if (!updateVersions.Contains(patchVersion))
                    {
                        updateVersions.Add(patchVersion);
                    }
                    patchCount++;
                }
                if (task.updateMainBuildRequired)
                    result.updateStatus = UpdateStatus.Main_Build_Update_Required;
            }

            if (updateVersions.Any())
            {
                updateVersions.Sort();
                updateVersions.Reverse();    
                var releaseNotes = updateVersions.Where(x => applicationRemoteData.ReleaseNotes.Any(rn => rn.Version == x))
                    .Select(x => applicationRemoteData.ReleaseNotes.Find(rn => rn.Version == x))
                    .Where(x => x != null);
                var releaseNotesBuilder = new StringBuilder();
                foreach (var releaseNote in releaseNotes)
                {
                    releaseNotesBuilder.AppendFormat("Version: {0}", releaseNote.Version);
                    releaseNotesBuilder.AppendLine();
                    releaseNotesBuilder.Append("      ").AppendLine(releaseNote.Notes);
                }
                result.notes = releaseNotesBuilder.ToString();
            }

            result.updateSize = updateSize;
            result.updateCount = patchCount;
            return result;
        }

        List<PackageTask> GetPackageTasks(ApplicationRemoteData applicationRemoteData, Version targetVersion)
        {
            var applicationData = GetApplicationData();
            var tasks = new List<PackageTask>();
            if (applicationData != null)
            {

                foreach (var package in applicationData.packages)
                {
                    var remotePackage = applicationRemoteData.Packages.FirstOrDefault(x => x.Name == package.name);
                    if (remotePackage == null)
                        continue;
                    var remotePatch = remotePackage.Patches.FirstOrDefault(x => x.Version == package.Version);
                    ///Check integrity
                    if (remotePatch != null)
                    {
                        var files = Directory.GetFiles(targetDirectory, "*", SearchOption.AllDirectories).Where(x=>!x.Contains("SGPatcherSources")).ToArray();
                        bool isBroken = false;
                        foreach (var filePath in files)
                        {
                            var fileName1 = Path.GetFileName(filePath);
                            var fileNames = files.Select(x => Path.GetFileName(x)).ToList();

                            foreach (var remoteFile in remotePatch.Files)
                            {
                                if (fileName1 == remoteFile.Filepath)
                                {
                                    Debug.Log($"[SG Patcher] - Checking file {fileName1}...");
                                    if (HashHelper.GetHashFromFile(filePath) != remoteFile.Hash)
                                    {
                                        Debug.LogError($"[SG Patcher] - File is broken {fileName1}!");
                                        Debug.Log($"[SG Patcher] - Starting reinstall process...");
                                        isBroken = true;
                                        break;
                                    }
                                }
                                if (!fileNames.Contains(remoteFile.Filepath))
                                {
                                    isBroken = true;
                                    break;
                                }
                            }

                        }
                        if (isBroken)
                        {
                            var packageTasks = GetNewPackageTasks(package.name, applicationRemoteData, targetVersion);
                            if (packageTasks.Count > 0)
                            {
                                tasks.AddRange(packageTasks);
                            }
                            continue;
                        }
                    }

                    var packageTask = GetPackageTask(package, applicationRemoteData, targetVersion);
                    if (packageTask != null)
                    {
                        if (packageTask.requiredPackages != null && packageTask.requiredPackages.Count > 0)
                        {
                            foreach (var packageName in packageTask.requiredPackages.Where(x => applicationData.packages.All(y => y.name == x)))
                            {
                                var requiredPackageTask = GetPackageTask(new PackageInfo { name = packageName, Version = Version.Empty }, applicationRemoteData, targetVersion);
                                if (requiredPackageTask != null)
                                {
                                    Debug.Log($"[SG Patcher] - Package to queue {requiredPackageTask.name}[{requiredPackageTask.version}]...");
                                    tasks.Add(requiredPackageTask);
                                }
                            }
                        }
                        Debug.Log($"[SG Patcher] - Package to queue {packageTask.name}[{packageTask.version}]...");
                        tasks.Add(packageTask);
                    }
                }

            }
            else
            {
                var packageTasks = GetNewPackageTasks(Consts.MAIN_PACKAGE_NAME, applicationRemoteData, targetVersion);
                if (packageTasks.Count != 0)
                {
                    tasks.AddRange(packageTasks);
                }
            }
            return tasks;
        }
        List<PackageTask> GetNewPackageTasks(string packageName, ApplicationRemoteData applicationRemoteData, Version targetVersion)
        {
            var tasks = new List<PackageTask>();
            var packageTask = GetPackageTask(new PackageInfo { name = packageName, Version = Version.Empty }, applicationRemoteData, targetVersion);
            if (packageTask != null)
            {
                if (packageTask.requiredPackages != null && packageTask.requiredPackages.Count > 0)
                {
                    foreach (var requiredPackageName in packageTask.requiredPackages)
                    {
                        var requiredPackageTask = GetPackageTask(new PackageInfo { name = requiredPackageName, Version = Version.Empty }, applicationRemoteData, targetVersion);
                        if (requiredPackageTask != null)
                        {
                            Debug.Log($"[SG Patcher] - Package to queue {requiredPackageTask.name}[{requiredPackageTask.version}]...");
                            tasks.Add(requiredPackageTask);
                        }
                    }
                }
                Debug.Log($"[SG Patcher] - Package to queue {packageTask.name}[{packageTask.version}]...");
                tasks.Add(packageTask);
            }
            return tasks;
        }

        public Task Update()
        {
            return Update(Version.Empty);
        }
        public async Task Update(Version targetVersion)
        {
            Debug.Log("[SG Patcher] - Starting update...");
            var applicationRemoteData = await GetApplicationRemoteData();
            Debug.Log("[SG Patcher] - Getting local application data...");
            var tasks = GetPackageTasks(applicationRemoteData, targetVersion);
            var storage = Storage.Get();
            var countPatches = tasks.Sum(x => x.patches.Count);
            deltaProgress = 1f / (float)(countPatches * 2);
            foreach (var task in tasks)
            {
                try
                {
                    await ApplyTask(task);
                }
                catch (FileHashesDontMatch)
                {
                    Debug.LogError($"[SG Patcher] - Package {task.name} broken while update!");
                    Debug.Log($"[SG Patcher] - Starting reinstall package process...");
                    var packageTasks = GetNewPackageTasks(task.name, applicationRemoteData, targetVersion);
                    foreach (var newPackageTask in packageTasks)
                    {
                        await ApplyTask(newPackageTask);
                    }
                }
            }

        }
        async Task ApplyTask(PackageTask task)
        {
            Debug.Log($"[SG Patcher] - Apply task {task.name}[{task.version}]...");
            foreach (var patchMeta in task.patches)
            {
                patchMeta.PackageName = task.name;
                await ApplyPatch(patchMeta);
            }
            var applicationData = GetApplicationData();
            if (applicationData == null)
            {
                applicationData = new ApplicationData();
            }
            var packageInfo = applicationData.packages.Find(x => x.name == task.name);
            if (packageInfo == null)
            {
                packageInfo = new PackageInfo { name = task.name };
                applicationData.packages.Add(packageInfo);
            }
            packageInfo.Version = task.version.ToVersion();
            var maxVersion = applicationData.packages.Max(x => x.Version);
            applicationData.Version = maxVersion;
            SaveApplicationData(applicationData);

        }
        async Task ApplyPatch(PatchMeta patchMeta, bool retry = false)
        {
            Debug.Log($"[SG Patcher] - Apply patch {patchMeta.Name}[{patchMeta.Version}]...");
            string targetPath = targetDirectory;
            if (needFolderForEachPackage)
            {
               targetPath = Path.Combine(targetDirectory, patchMeta.PackageName);
            }
            

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            var patchApplier = new PatchApplier();
            patchApplier.onProgress += PatchApplier_onProgress;
            if (patchMeta.IsInitial)
            {
                var patchFile = Path.Combine(TempHelper.GetTempFolder(), patchMeta.FullHash);
                storage.toRecieveBytes = patchMeta.FullSize;
                Debug.Log($"[SG Patcher] - Downloading file {patchMeta.FullId}[{patchMeta.FullSize.NormalizeFileSize()}]...");
                await storage.DownloadFile(patchMeta.FullId, patchFile, patchMeta.FullHash);
                mainProgress += deltaProgress;
                if (patchMeta.FullHash == HashHelper.GetHashFromFile(patchFile))
                {
                    Debug.Log($"[SG Patcher] - Apply full patch to {targetPath}...");
                    await patchApplier.ApplyFull(patchFile, targetPath);
                    mainProgress += deltaProgress;
                }
                else
                {
                    Debug.LogError($"[SG Patcher] - Downloading error: Incorrect file hash. ");
                    File.Delete(patchFile);
               
                    if (!retry)
                    {     
                        retry = true;
                        Debug.Log($"[SG Patcher] - Trying patch again to {targetPath}...");
                        await ApplyPatch(patchMeta, retry);
                    }
                    else
                    {
                        throw new System.ApplicationException($"[SG Patcher] - Unable to apply the update even after trying again, please contact technical support. Package: {patchMeta.PackageName}. Version: {patchMeta.Version}.");
                    }
                }
                if (File.Exists(patchFile))
                {
                    File.Delete(patchFile);
                }
            }
            else
            {
                var patchFile = Path.Combine(TempHelper.GetTempFolder(), patchMeta.PatchHash);
                storage.toRecieveBytes = patchMeta.PatchSize;
                Debug.Log($"[SG Patcher] - Downloading file {patchMeta.PatchId}[{patchMeta.PatchSize.NormalizeFileSize()}]...");
                await storage.DownloadFile(patchMeta.PatchId, patchFile, patchMeta.PatchHash);
                mainProgress += deltaProgress;
                if (patchMeta.PatchHash == HashHelper.GetHashFromFile(patchFile))
                {
                    Debug.Log($"[SG Patcher] - Apply patch to {targetPath}...");
                    await patchApplier.ApplyPatch(patchFile, targetPath);
                    mainProgress += deltaProgress;
                }
                else
                {
                    Debug.LogError($"[SG Patcher] - Downloading error: Incorrect file hash. ");
                    File.Delete(patchFile);
                    retry = true;
                    if (!retry)
                    {
                        Debug.Log($"[SG Patcher] - Trying patch again to {targetPath}...");
                        await ApplyPatch(patchMeta, retry);
                    }
                    else
                    {
                        throw new System.ApplicationException($"[SG Patcher] - Unable to apply the update even after trying again, please contact technical support. Package: {patchMeta.PackageName}. Version: {patchMeta.Version}.");
                    }
                }
                if (File.Exists(patchFile))
                {
                    File.Delete(patchFile);
                }

            }
        }

        public Task InstallPackage(string packageName)
        {
            return InstallPackage(packageName, Version.Empty);
        }
        public async Task InstallPackage(string packageName, Version targetVersion)
        {
            var applicationRemoteData = await GetApplicationRemoteData();
            Debug.Log("[SG Patcher] - Getting local application data...");
            var applicationData = GetApplicationData();
            var tasks = new List<PackageTask>();
            if (applicationData != null)
            {
                var package = applicationData.packages.FirstOrDefault(x => x.name == packageName);
                if (package == null)
                    package = new PackageInfo { name = packageName, Version = Version.Empty };
                var packageTask = GetPackageTask(package, applicationRemoteData, targetVersion);
                if (packageTask != null)
                {
                    if (packageTask.requiredPackages != null && packageTask.requiredPackages.Count > 0)
                    {
                        foreach (var requiredPackageName in packageTask.requiredPackages.Where(x => applicationData.packages.All(y => y.name == x)))
                        {
                            var requiredPackageTask = GetPackageTask(new PackageInfo { name = requiredPackageName, Version = Version.Empty }, applicationRemoteData, targetVersion);
                            if (requiredPackageTask != null)
                            {
                                Debug.Log($"[SG Patcher] - Package to queue {requiredPackageTask.name}[{requiredPackageTask.version}]...");
                                tasks.Add(requiredPackageTask);
                            }
                        }
                    }
                    Debug.Log($"[SG Patcher] - Package to queue {packageTask.name}[{packageTask.version}]...");
                    tasks.Add(packageTask);
                }
                var countPatches = tasks.Sum(x => x.patches.Count);
                deltaProgress = 1f / (float)(countPatches * 2);
                foreach (var task in tasks)
                {
                    await ApplyTask(task);
                }
            }
            else
            {
                Debug.LogError($"[SG Patcher] - You should install {Consts.MAIN_PACKAGE_NAME} before install package...");
            }
        }

        public Task InstallAllPackages()
        {
            return InstallAllPackages(Version.Empty);
        }
        public async Task InstallAllPackages(Version targetVersion)
        {
            var applicationRemoteData = await GetApplicationRemoteData();
            foreach (var package in applicationRemoteData.Packages)
            {
                if (package.Name != "Main")
                {
                    await InstallPackage(package.Name, targetVersion);
                }
            }
        }
        void PatchApplier_onProgress(float progress, string status)
        {
            onProgressChanged?.Invoke(new PatcherProgress
            {
                status = status,
                progress = mainProgress + progress * deltaProgress
            });
        }


    }
}