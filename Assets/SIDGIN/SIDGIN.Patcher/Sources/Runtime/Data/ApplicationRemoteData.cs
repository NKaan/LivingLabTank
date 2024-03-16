using System.Collections.Generic;
using System.Xml.Serialization;
namespace SIDGIN.Patcher.Client
{
    public class ApplicationRemoteData
    {

        public ApplicationRemoteData() { }

        [XmlArray("Packages")]
        [XmlArrayItem("Package")]
        public List<PackageData> Packages { get; set; } = new List<PackageData>();
        [XmlElement("ReleaseNotes")]

        public List<ReleaseNotesData> ReleaseNotes { get; set; } = new List<ReleaseNotesData>();

    }

    public class PackageData
    {
        public PackageData() { }
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlArray("RequiredPackages")]
        [XmlArrayItem("Package")]
        public List<string> RequiredPackages { get; set; } = new List<string>();
        [XmlArray("Patches")]
        [XmlArrayItem("Patch")]
        public List<PatchMeta> Patches { get; set; } = new List<PatchMeta>();
    }
}