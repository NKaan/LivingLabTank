using SIDGIN.Common.Editors;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    public class UploadHistoryView
    {
        string downloadPath;
        List<PackageUploadTask> tasks;
        Vector2 scroll;
        public void OnEnable()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            if (settingsData.SelectedDefinition == null)
                return;
            LoadMeta();
        }
        public void OnDraw()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            EditorGUILayout.BeginHorizontal("Toolbar");
            EditorGUILayout.LabelField("Version name");
            EditorGUILayout.EndHorizontal();
            if (tasks != null)
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                foreach (var task in tasks.ToArray())
                {
                    DrawFile(task);
                }
                EditorGUILayout.EndScrollView();
            }
        }
        void DrawFile(PackageUploadTask task)
        {
            EditorGUILayout.BeginHorizontal("HelpBox");
            EditorGUILayout.SelectableLabel(task.PackageName);
            EditorGUILayout.SelectableLabel(task.Version);
            if (GUILayout.Button("Upload", "PreButton", GUILayout.Width(100), GUILayout.Height(20)))
            {
                UploadVersion(task);
            }
            if (GUILayout.Button("Delete", "PreButton", GUILayout.Width(100), GUILayout.Height(20)))
            {
                Delete(task);
            }
            EditorGUILayout.EndHorizontal();

        }

        async void UploadVersion(PackageUploadTask task)
        {
            var packageManager = new PackageManagerEditor();
            MainWindow.IsBusy = true;
            packageManager.onProcessChanged += OnProgressChanged;
            await packageManager.PutPackage(task);
            OnUploadComplete();
            if (tasks.Count == 0)
                MainWindow.ShowHome();
        }


        void LoadMeta()
        {
            MainWindow.OnProgressChanged(0, $"Loading...");
            var assetBundlesForDefinition = EditorSettingsLoader.Get<BuildSettingsCacheData>().GetSelectedData();
            tasks = assetBundlesForDefinition.packageUploadTasks;
            MainWindow.ResetProgress();
            MainWindow.DoRepaint();
        }

        void Delete(PackageUploadTask version)
        {
            MainWindow.OnProgressChanged(0, $"Process deleting...");
            
            MainWindow.ResetProgress();
            tasks.Remove(version);
            var assetBundlesData = EditorSettingsLoader.Get<BuildSettingsCacheData>();
            assetBundlesData.Save();
            if (tasks.Count == 0)
                MainWindow.ShowHome();
        }

        void OnProgressChanged(float progress, string status)
        {
            status = $"Uploading file {(progress * 100).ToString("00")}%...";
            MainWindow.OnProgressChanged(progress, status);
            SharedProgressBar.DisplayProgress(status, progress);
        }
        void OnUploadComplete()
        {
            MainWindow.ResetProgress();
            SharedProgressBar.ClearProgress();
        }
    }
}
