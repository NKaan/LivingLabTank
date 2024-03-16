using SIDGIN.Common.Editors;
using SIDGIN.Patcher.Client;
using SIDGIN.Patcher.Common;
using SIDGIN.Patcher.Editors.Storages;
using SIDGIN.Patcher.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Version = SIDGIN.Patcher.Client.Version;

namespace SIDGIN.Patcher.Editors
{
    public class PackageManagerEditor
    {
        ApplicationRemoteData applicationRemoteData;
        public event Action<float, string> onProcessChanged;

        public async Task<ApplicationRemoteData> GetRemoteData()
        {
            if (applicationRemoteData != null)
                return applicationRemoteData;
            try
            {
                var storageApi = Storage.Get();
                var data = await storageApi.DownloadDataByName(Consts.APP_DATA_FILE_NAME);
                var applicationData = XmlSerializeHelper.Deserialize<ApplicationRemoteData>(data);
                applicationRemoteData = applicationData;
                return applicationData;
            }
            catch (UnableLoadResource ex)
            {
                return null;
            }
        }
        public async Task<PackageData> GetPackage(string packageName)
        {
            var applicationData = await GetRemoteData();
            if(applicationData == null)
                return  null;
            var packageMeta = applicationData.Packages.FirstOrDefault(x => x.Name == packageName);
            return packageMeta;
        }

        public async Task<Version> GetLastPackagesVersion()
        {
            var applicationRemoteData = await GetRemoteData();
            if(applicationRemoteData == null)
                return  Version.Empty;
            
            Version lastVersion = Version.Empty;
            foreach (var package in applicationRemoteData.Packages)
            {
                foreach (var patch in package.Patches)
                {
                    if (lastVersion < patch.Version)
                    {
                        lastVersion = patch.Version;
                    }
                }
            }

            return lastVersion;
        }
        
        string uploadStatus;
        public async Task PutPackage(PackageUploadTask packageToUpload)
        {
            var applicationData = await GetRemoteData();
            if(applicationData == null)
              applicationData = new ApplicationRemoteData();
            
            var storageApi = Storage.Get();
            storageApi.onProgressChanged += data => onProcessChanged?.Invoke(data.progress, uploadStatus);
            string fullUrl = "";
            long fullSize = 0;
            string fullHash = "";
            if (File.Exists(packageToUpload.FullPath))
            {
                fullHash = HashHelper.GetHashFromFile(packageToUpload.FullPath);
                fullSize = new FileInfo(packageToUpload.FullPath).Length;
                string fileName = $"FULL_{packageToUpload.PackageName}[{packageToUpload.Version}].fullpackage";
                uploadStatus = $"Uploading {fileName} [{fullSize.NormalizeFileSize()}]";
                fullUrl = await storageApi.UploadFile(packageToUpload.FullPath, fileName);
                try
                {
                    File.Delete(packageToUpload.FullPath);
                }
                catch
                {
                }
            }

            string patchUrl = "";
            long patchSize = 0;
            string patchHash = "";
            if (File.Exists(packageToUpload.PatchPath)) 
            {
                patchHash = HashHelper.GetHashFromFile(packageToUpload.PatchPath);
                patchSize = new FileInfo(packageToUpload.PatchPath).Length;
                string fileName = $"PATCH_{packageToUpload.PackageName}[{packageToUpload.Version}].patchpackage";
                uploadStatus = $"Uploading {fileName} [{fullSize.NormalizeFileSize()}]";
                patchUrl = await storageApi.UploadFile(packageToUpload.PatchPath, fileName);
                try
                {
                    File.Delete(packageToUpload.PatchPath);
                }
                catch
                {
                }
            }
           
            var packageData = applicationData.Packages.FirstOrDefault(x => x.Name == packageToUpload.PackageName);
            if(packageData == null)
            {
                packageData = new PackageData { Name = packageToUpload.PackageName};
                applicationData.Packages.Add(packageData);
            }
            packageData.RequiredPackages = packageToUpload.requiredPackages;
            var patchMeta = new PatchMeta
            {
                Version = packageToUpload.Version.ToVersion(),
                IsInitial = packageToUpload.IsFullPackage,
                MainVersion = packageToUpload.MainVersion,
                Files = packageToUpload.Files,
                FullId = fullUrl,
                FullSize = fullSize,
                FullHash = fullHash,
                PatchId = patchUrl,
                PatchSize = patchSize,
                PatchHash = patchHash,
                
            };
            packageData.Patches.RemoveAll(x => x.Version == packageToUpload.Version.ToVersion());
            packageData.Patches.Add(patchMeta);   
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            if (settingsData.patchTask != null && !string.IsNullOrEmpty(settingsData.patchTask.notes))
            {
                if (applicationData.ReleaseNotes != null)
                {
                    if (applicationData.ReleaseNotes.All(x => x.Version != settingsData.patchTask.version.ToVersion()))
                    {
                        applicationData.ReleaseNotes.Add(new ReleaseNotesData
                            {
                                Version = settingsData.patchTask.version.ToVersion(), 
                                Notes = settingsData.patchTask.notes
                            });
                    }
                }
                else
                {
                    applicationData.ReleaseNotes = new List<ReleaseNotesData>
                    {
                        new ReleaseNotesData
                        {
                            Version = settingsData.patchTask.version.ToVersion(), 
                            Notes = settingsData.patchTask.notes
                        }
                    };
                }
            }

            var serializedData = XmlSerializeHelper.Serialize(applicationData);
            var resultId = await storageApi.UploadData(serializedData, Consts.APP_DATA_FILE_NAME);
            var clientSettings = EditorSettingsLoader.Get<ClientSettings>(true);
            clientSettings.appId = resultId;
            clientSettings.Save();
        
            settingsData.SelectedDefinition.versionsFileId = await storageApi.GetVersionsLink();
            settingsData.Save();
        }

        public async Task DeletePatch(PatchMeta patch)
        {
            var applicationData = await GetRemoteData();
            if (applicationData == null)
                return;

            var package = applicationData.Packages.FirstOrDefault(x => x.Name == patch.PackageName);
            if (package != null)
            {
                var storageApi = Storage.Get();
                var remotePatchMeta = package.Patches.FirstOrDefault(x => x.Version == patch.Version);
                if (!string.IsNullOrEmpty(remotePatchMeta.FullId))
                {
                    await storageApi.DeleteFile(remotePatchMeta.FullId);
                }
                else if (!string.IsNullOrEmpty(remotePatchMeta.PatchId))
                {
                    await storageApi.DeleteFile(remotePatchMeta.PatchId);
                }

                package.Patches.Remove(remotePatchMeta);
                var serializedData = XmlSerializeHelper.Serialize(applicationData);
                await storageApi.UploadData(serializedData, Consts.APP_DATA_FILE_NAME);
            }
        }

        public async Task DeleteFullPatch(PatchMeta patch)
        {
            var applicationData = await GetRemoteData();
            if (applicationData == null)
                return;
            var package = applicationData.Packages.FirstOrDefault(x => x.Name == patch.PackageName);
            if (package != null)
            {
                var storageApi = Storage.Get();
                var remotePatchMeta = package.Patches.FirstOrDefault(x => x.Version == patch.Version);
                if (!string.IsNullOrEmpty(remotePatchMeta.FullId))
                {
                    await storageApi.DeleteFile(remotePatchMeta.FullId);
                }

                remotePatchMeta.FullId = "";
                remotePatchMeta.FullSize = 0;
                remotePatchMeta.FullHash = "";
                remotePatchMeta.IsInitial = false;
                //if (string.IsNullOrEmpty(remotePatchMeta.PatchId))
                //{
                //    package.Patches.Remove(remotePatchMeta);
                //}
                var serializedData = XmlSerializeHelper.Serialize(applicationData);
                await storageApi.UploadData(serializedData, Consts.APP_DATA_FILE_NAME);
            }
        }

        public async Task DeletePackages(Version version)
        {
            var applicationData = await GetRemoteData();
            if (applicationData == null)
                return;
            var storageApi = Storage.Get();
            foreach (var package in applicationData.Packages)
            {
                foreach (var patch in package.Patches.Where(x => x.Version == version && !x.IsInitial).ToArray())
                {
                    if (!string.IsNullOrEmpty(patch.FullId))
                    {
                        await storageApi.DeleteFile(patch.FullId);
                    }
                    else if (!string.IsNullOrEmpty(patch.PatchId))
                    {
                        await storageApi.DeleteFile(patch.PatchId);
                    }

                    package.Patches.Remove(patch);
                }
            }

            var serializedData = XmlSerializeHelper.Serialize(applicationData);
            await storageApi.UploadData(serializedData, Consts.APP_DATA_FILE_NAME);
        }
    }
}
