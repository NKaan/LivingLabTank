
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
namespace SIDGIN.Patcher.Client
{
    public class PatchMeta : IComparable<PatchMeta>
    {
        public PatchMeta() { }

        [XmlIgnore]
        public string PackageName { get; set; }

        [XmlAttribute(AttributeName = "Version")]
        public string VersionStr
        {
            get { return Version.ToString(); }
            set { Version = value.ToVersion(); }
        }
        [XmlIgnore]
        public Version Version { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string MainVersion { get; set; }
        [XmlAttribute]
        public bool IsInitial { get; set; }

        [XmlArray("Files")]
        [XmlArrayItem("File")]
        public List<HashFileInfo> Files { get; set; }

        [XmlAttribute]
        public string PatchId { get; set; }
        [XmlAttribute]
        public string PatchHash { get; set; }
        [XmlAttribute]
        public long PatchSize { get; set; }

        [XmlAttribute]
        public string FullId { get; set; }
        [XmlAttribute]
        public string FullHash { get; set; }
        [XmlAttribute]
        public long FullSize { get; set; }

        public int CompareTo(PatchMeta other)
        {
            return this.Version.CompareTo(other.Version);
        }
    }
}