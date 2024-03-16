using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace SIDGIN.Patcher.Client
{
    [System.Serializable]
    public class ReleaseNotesData
    {
        [XmlAttribute(AttributeName = "Version")]
        public string VersionStr
        {
            get { return Version.ToString(); }
            set { Version = value.ToVersion(); }
        }
        [XmlIgnore]
        public Version Version { get; set; }

        [XmlElement] 
        public string Notes { get; set; }
    }
}