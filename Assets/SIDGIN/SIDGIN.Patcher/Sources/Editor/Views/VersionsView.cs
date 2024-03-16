using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

namespace SIDGIN.Patcher.Editors
{
    using SIDGIN.Common.Editors;
    using SIDGIN.Patcher.Client;
    using SIDGIN.Patcher.Storages;
    using System.Collections.Generic;
    using System.Linq;

    public class VersionsView
    {
        
        ApplicationRemoteData applicationData;
        List<VersionData> versionDatas;
        public Version LastVersion
        {
            get
            {
                if(versionDatas != null)
                {
                    var lastVersionData = versionDatas.LastOrDefault();
                    if(lastVersionData != null)
                    {
                        return lastVersionData.Version;
                    }
                }
                return Version.Empty;
            }
        }
        class VersionData
        {
            public string VersionStr;
            public Version Version;
            public bool isExpand;
            public List<PatchMeta> patches = new List<PatchMeta>();
        }
        Vector2 scrollPosition;
        Version actualInitialVersion;
        public async Task UpdateData()
        {
            await UpdateVersionList();
        }  
        public void OnDraw()
        {
            
            if (applicationData == null || versionDatas == null)
            {
                return;
            }
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            int index = 0;
            foreach (var versionData in versionDatas)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                versionData.isExpand = EditorGUILayout.Foldout(versionData.isExpand, versionData.VersionStr);
                if (versionData.patches.Any(x=>x.IsInitial))
                    EditorGUILayout.LabelField("Full version");
                if ((index == 0 || index == versionDatas.Count - 1) && !versionData.patches.All(x=>x.IsInitial) && GUILayout.Button("Delete", "toolbarbutton",GUILayout.Width(70)))
                {
                    DeleteVersion(versionData.Version);
                }
                EditorGUILayout.EndHorizontal();
                if (versionData.isExpand)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    int indexPatch = 0;
                    foreach(var patch in versionData.patches)
                    {
                        bool isRemoved = string.IsNullOrEmpty(patch.PatchId) && string.IsNullOrEmpty(patch.FullId);
                        EditorGUI.BeginDisabledGroup(isRemoved);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(patch.PackageName);
                  
                        
                        if (patch.IsInitial)
                        {
                            if (patch.FullSize != 0)
                            {
                                EditorGUILayout.LabelField(patch.FullSize.NormalizeFileSize());
                            }
                            EditorGUILayout.LabelField("Full version");
                        }
                        else
                        {
                            if (patch.PatchSize != 0)
                            {
                                EditorGUILayout.LabelField(patch.PatchSize.NormalizeFileSize());
                            }
                        }
                        
                       
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        indexPatch++;
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }
                index++;
            }
            EditorGUILayout.EndScrollView();
            

        }

        async void DeleteVersion(Version version)
        {
            MainWindow.OnProgressChanged(0, "Delete version...");
            try
            {
                var packageManager = new PackageManagerEditor();
                await packageManager.DeletePackages(version);
            }
            catch (UnableLoadResource ex)
            {
                Debug.LogWarning(ex);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }

            await UpdateVersionList();
            MainWindow.ResetProgress();
        }
        async Task UpdateVersionList()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            if (string.IsNullOrEmpty(settingsData.SelectedDefinition?.versionsFileId))
            {
                applicationData = null;
                return;
            }
            MainWindow.OnProgressChanged(0, "Loading versions...");
            try
            {
                var packageManager = new PackageManagerEditor();
                applicationData = await packageManager.GetRemoteData();
                if (applicationData != null)
                {
                    versionDatas = new List<VersionData>();
                    foreach (var package in applicationData.Packages)
                    {
                        foreach(var patch in package.Patches)
                        {
                            var versionData = versionDatas.FirstOrDefault(x => x.Version == patch.Version);
                            if(versionData == null)
                            {
                                versionData = new VersionData { Version = patch.Version, VersionStr = patch.VersionStr };
                                versionDatas.Add(versionData);
                            }
                            patch.PackageName = package.Name;
                            versionData.patches.Add(patch);
                        }
                    }
                    versionDatas =  versionDatas.OrderByDescending(x => x.Version).ToList();
                    //var versionsControl = new VersionsControlEditor(ConfigurationHelper.GetConfig());
                    //versionList = await versionsControl.GetVersionList();
                    //if (versionList != null)
                    //{
                    //    var actualVersionMeta = versionList.Versions.FindLast(x => x.IsInitial);
                    //    if (actualVersionMeta != null)
                    //    {
                    //        actualInitialVersion = actualVersionMeta.Name.ToVersion();
                    //    }
                    //}
                    settingsData.Save();
                }
            }
            catch (UnableLoadResource ex)
            {
                Debug.LogWarning(ex);
                applicationData = null;
            }
            catch(System.Exception ex)
            {
                Debug.LogError(ex);
                applicationData = null;
            }
            MainWindow.ResetProgress();
            MainWindow.DoRepaint();
        }
        

        void OnVersionBuildingProgressChanged(float p, string s)
        {
            SharedProgressBar.DisplayProgress("Change initial version...", p);
            MainWindow.OnProgressChanged(p, s);
        }

    }
}