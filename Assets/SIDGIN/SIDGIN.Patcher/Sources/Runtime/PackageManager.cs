using SIDGIN.Patcher.Common;
using SIDGIN.Patcher.Storages;
using System.Linq;
using System.Threading.Tasks;

namespace SIDGIN.Patcher.Client
{
    public class PackageManager
    {
        
        public async Task<ApplicationRemoteData> GetRemoteData()
        {
            var storageControl = new StorageControl();
            var patcherClientSettings = ClientSettings.Get();
            var data = await storageControl.DownloadData(patcherClientSettings.appId);
            var applicationData = XmlSerializeHelper.Deserialize<ApplicationRemoteData>(data);
            return applicationData;
        }
        public async Task<PackageData> GetPackage(string packageName)
        {
            var applicationData = await GetRemoteData();
            var packageMeta = applicationData.Packages.FirstOrDefault(x => x.Name == packageName);
            return packageMeta;
        }

    }
}
