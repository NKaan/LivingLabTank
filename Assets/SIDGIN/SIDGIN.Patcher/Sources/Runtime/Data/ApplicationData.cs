
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SIDGIN.Patcher.Client
{
    public class PackageInfo
    {
        public string name;

        [XmlAttribute(AttributeName = "Version")]
        public string VersionStr
        {
            get { return Version.ToString(); }
            set { Version = value.ToVersion(); }
        }
        [XmlIgnore]
        public Version Version { get; set; }
    }
    public class ApplicationData
    {
        [XmlElement(ElementName = "Version")]
        public string VersionStr
        {
            get { return Version.ToString(); }
            set { Version = value.ToVersion(); }
        }
        [XmlIgnore]
        public Version Version { get; set; }

        [XmlArray("Packages")]
        [XmlArrayItem("Package")]
        public List<PackageInfo> packages = new List<PackageInfo>();
    }
}
