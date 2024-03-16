using System.Xml.Serialization;
using UnityEngine;

namespace SIDGIN.Patcher.Client
{ 
    [System.Serializable]
    public class HashFileInfo
    {
        public HashFileInfo() { }

        [SerializeField]
        string filePath;

        [XmlAttribute]
        public string Filepath { get { return filePath; } set { filePath = value; } }

        [SerializeField]
        string hash;

        [XmlAttribute]
        public string Hash { get { return hash; } set { hash = value; } }
    }
}