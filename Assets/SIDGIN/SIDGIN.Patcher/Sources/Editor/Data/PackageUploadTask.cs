using SIDGIN.Patcher.Client;
using System.Collections.Generic;

namespace SIDGIN.Patcher.Editors
{
    public class PackageUploadTask
    {
        public string PackageName;
        public string VersionFrom;
        public string Version;
        public string FullPath;
        public string PatchPath;
        public bool IsFullPackage;
        public string MainVersion;
        public List<HashFileInfo> Files;
        public List<string> requiredPackages;
    }
}
