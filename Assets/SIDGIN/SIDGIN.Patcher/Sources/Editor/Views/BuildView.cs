using UnityEngine;
using UnityEditor;

namespace SIDGIN.Patcher.Editors
{
    using Client;
    using SIDGIN.Common.Editors;
    using System.IO;
    using System.Threading.Tasks;

    public class BuildView
    {
        static BuildView instance;
        VersionsView versionsView = new VersionsView();
        SettingsData settingsData;
        Version version;
        Version lastVersion;
        string notes;
        bool isInitial;
        bool IsNextVersionValid
        {
            get
            {
                return version > lastVersion;
            }
        }
        public async void OnEnable()
        {
            isInitial = false;
            settingsData = EditorSettingsLoader.Get<SettingsData>();
            instance = this;
            await UpdateVersionData();
            ClientSettingsSaver.SaveClientSettings();
            if (settingsData.patchTask != null && settingsData.patchTask.needPatch)
                return;
            
        }
        async Task UpdateVersionData()
        {
            await versionsView.UpdateData();
            lastVersion = versionsView.LastVersion;
            version = lastVersion + 1;
        }
        public void OnDraw()
        {
            var defNames = settingsData.DefinitionNames;
            if (defNames == null || defNames.Count == 0)
            {
                EditorGUILayout.HelpBox("You must add at least one definition before creating versions. Go to settings.",MessageType.Warning);
                return;
            }
            EditorGUILayout.BeginHorizontal("Toolbar");
            EditorGUI.BeginDisabledGroup(!IsNextVersionValid);
            if (GUILayout.Button("Publish", "toolbarbutton", GUILayout.Width(100)))
            {
                EditorDispatcher.Dispatch(Publish);
            }
            isInitial = EditorGUILayout.ToggleLeft("Full version", isInitial);
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            DrawDefinitionSelector();
            EditorGUILayout.EndHorizontal();
            DrawVersionSelector();
            versionsView.OnDraw();

        }
   
        void DrawVersionSelector()
        {

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Version: ", GUILayout.Width(70));

            version = VersionField.Show(version);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Release Notes:");
            notes = EditorGUILayout.TextArea(notes,GUILayout.MaxHeight(50));
            if (!IsNextVersionValid)
            {
                EditorGUILayout.HelpBox("The next version should be larger!", MessageType.Error);
            }
        }
        void DrawDefinitionSelector()
        {
            EditorGUILayout.LabelField("Definition: ", GUILayout.Width(60));
            var defNames = settingsData.DefinitionNames;
            if (defNames.Count != 0)
            {
                EditorGUI.BeginChangeCheck();
                var index = EditorGUILayout.Popup(defNames.IndexOf(settingsData.selectedDefinition), defNames.ToArray());
                if (index != -1)
                {
                    settingsData.selectedDefinition = defNames[index];
                }
                else
                {
                    settingsData.selectedDefinition = "NONE";
                }
                if (EditorGUI.EndChangeCheck())
                {
                    OnEnable();
                }

            }
            else
            {
                EditorGUILayout.LabelField("Empty list", GUILayout.Width(100));
            }
        }

        void Publish()
        {
            MainWindow.IsBusy = true;
            settingsData.patchTask = new PatchTask
            {
                needPatch = true,
                isInitial = isInitial,
                version = version.ToString(),
                notes = notes
            };
            Build();
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            DoPublish();
#endif
        }

        static void Build()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            var targetPath = Path.Combine(EditorConsts.VERSIONS_PATH, settingsData.patchTask.version);
            AssetBundlesBuildHelper.Build(targetPath);
        }
        static async void DoPublish()
        {
            var packageManager = new PublishManager();
            packageManager.onProcessChanged += MainWindow.OnProgressChanged;
            await packageManager.Publish();
            if (instance != null)
            {
                await instance.UpdateVersionData();
            }
            MainWindow.ResetProgress();
        }


        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptsReloaded()
        {
            DoPublish();
        }

      
        static void OnVersionBuildingProgressChanged(float p, string s)
        {
            MainWindow.OnProgressChanged(p, s);
            SharedProgressBar.DisplayProgress("Version building...", p);
        }



    }
}