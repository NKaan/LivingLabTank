using System.Collections.Generic;
using UnityEngine;

namespace SIDGIN.Patcher.Client
{
    [System.Serializable]
    public class SceneData
    {
        public bool IsActive { get; internal set; }
        public int index;
        public string name;
        public string sharedResources = "None";
    }
    public class RuntimeData : ScriptableObject
    {
        public List<SceneData> sceneDatas = new List<SceneData>();
        public List<string> sharedResources = new List<string>();
    }
}
