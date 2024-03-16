using System.Collections.Generic;
using System.Xml.Serialization;

namespace SIDGIN.Patcher.Client
{
    public class FileEntry
    {
        [XmlAttribute]
        public EntryType Type { get; set; }
        [XmlAttribute]
        public string Filename { get; set; }
        [XmlAttribute]
        public string HashNew { get; set; }
        [XmlAttribute]
        public string HashOld { get; set; }
    }
    public enum EntryType
    {
      Added,
      Modified,
      Removed
    }
    public class PatchEntry
    {
        [XmlAttribute]
        public int BlockSize { get; set; }
        [XmlElement("Files")]
        public List<FileEntry> FileEntries { get; set; }
       
    }
}
