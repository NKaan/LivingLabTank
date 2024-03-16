using System.Linq;
using UnityEngine;
namespace SIDGIN.Patcher.Editors
{
    using System.Collections.Generic;
    [System.Serializable]
    public class PatchTask 
    {
        public bool needPatch;
        public string version;
        public string notes;
        public bool isInitial;
    }
    public class SettingsData : ScriptableObject
    {
        public string selectedDefinition;
#if SGPATCHER_GOOGLE
        public string selectedStorageModule = "Google";
#else
        public string selectedStorageModule = "Http";
#endif
        public bool offlineMode;
        [Range(0,9)]
        public int zipCompressLevel = 9;

        [SerializeField]
        List<Definition> definitions = new List<Definition>();
        //[HideInInspector]
        public PatchTask patchTask;
        public List<Definition> Definitions { get { return definitions; } }

        public List<Definition> DefinitionsOfModule { get { return definitions.Where(x => x.StorageName == selectedStorageModule).ToList(); } }

        public Definition SelectedDefinition { get { return definitions.FirstOrDefault(x => x.Name == selectedDefinition); } }

        public List<string> DefinitionNames
        {
            get
            {
                return definitions.Where(x => x.StorageName == selectedStorageModule)
                    .Select(x => x.Name)
                    .ToList();
            }
        }
        public void SelectDefaultDefinition()
        {
            var defOfModule = DefinitionsOfModule;
            if(defOfModule.Count == 0)
            {
                selectedDefinition = "";
            }
            else
            {
                selectedDefinition = defOfModule.First().Name;
            }
        }
        public void SetDefinition(string definitionName)
        {
            if (DefinitionNames.Contains(definitionName))
            {
                selectedDefinition = definitionName;
            }
        }
    }
}