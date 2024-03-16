using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

namespace SIDGIN.Patcher.Editors
{
    using SIDGIN.Common.Editors;
    using Storages;
    using System.Linq;

    public class DefinitionsView
    {
        Vector2 scroll;
        string definitionName;
        SettingsData settingsData;

        public void OnEnable()
        {
            settingsData = EditorSettingsLoader.Get<SettingsData>();
        }
        public void OnDraw()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Definitions", EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Definition name:", GUILayout.Width(100));

            definitionName = EditorGUILayout.TextField(definitionName);

            if (GUILayout.Button("Add", "toolbarbutton",GUILayout.Width(80)))
            {
                EditorDispatcher.Dispatch(() => Add(definitionName));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal("Toolbar");
            EditorGUILayout.LabelField("Name", GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Type", GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Delete",GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(100));
            var definitions = settingsData.DefinitionsOfModule;
            if (definitions.Count == 0)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("List is empty.",GUILayout.Width(90));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {

                scroll = EditorGUILayout.BeginScrollView(scroll);
                foreach (var def in definitions)
                {
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    EditorGUILayout.LabelField(def.Name);
                   
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Delete", "toolbarbutton"))
                    {
                        EditorDispatcher.Dispatch(() => Delete(def));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
         
        }
        void Delete(Definition def)
        {
            Undo.RecordObject(settingsData, "Delete definition");
            settingsData.Definitions.Remove(def);
            var definitionNames = settingsData.DefinitionNames;
            if (definitionNames.Any())
            {
                settingsData.selectedDefinition = definitionNames.FirstOrDefault();
            }
            var buildSettingsData = EditorSettingsLoader.Get<BuildSettingsCacheData>();
            buildSettingsData.RemoveSettings(def.Name);
            buildSettingsData.Save();
            settingsData.Save();
        }
        void Add(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                EditorUtility.DisplayDialog("Definition name is null", "Definition name and folder can't be null!", "OK");
                return;
            }
            if(settingsData.DefinitionsOfModule.Exists(x=>x.Name == name))
            {
                EditorUtility.DisplayDialog("Definition already exist!","A definition with that name or folder already exists.","OK");
                return;
            }
            Undo.RecordObject(settingsData, "Add definition");
            var defData = new Definition(name, settingsData.selectedStorageModule);
            UpdateDefinitionData(defData);
           
        }
        async void UpdateDefinitionData(Definition defData)
        { 
            var storageControl = new StorageControlEditor();
            await Task.Run(async () =>
            {
                MainWindow.OnProgressChanged(0, "Checking information...");
                defData.versionsFileId = await storageControl.GetVersionsLink();
           
                MainWindow.ResetProgress();

            });
           
            settingsData.selectedDefinition = defData.Name;
            settingsData.Definitions.Add(defData);
            settingsData.Save();
            definitionName = "";
        }
    }
}