using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SIDGIN.Patcher.Editors.Storages
{
    using SIDGIN.Common.Editors;
    using SIDGIN.Patcher.Storages;
    using SIDGIN.Patcher.Client;
    public class StorageView
    {

        string filePath;
        float progress;
        string status;
        string downloadPath;
        List<FileMeta> files;
        Vector2 scroll;
        StorageControlEditor storageControl;
        public void OnEnable()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            if (settingsData.SelectedDefinition == null)
                return;
            storageControl = new StorageControlEditor();
            storageControl.onProgressChanged += OnDownloadProgressChanged;
            EditorDispatcher.Dispatch(LoadMeta); 
            ClientSettingsSaver.SaveClientSettings();
        }
        public void OnDraw()
        {
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            if (settingsData.SelectedDefinition == null)
            {
                EditorGUILayout.HelpBox("You must add at least one definition. Go to settings.", MessageType.Warning);
                return;
            }
            EditorGUILayout.BeginHorizontal("Toolbar");
            EditorGUILayout.LabelField("File name");
            EditorGUILayout.LabelField("Size", GUILayout.Width(100));
            if (GUILayout.Button("Update", "toolbarbutton", GUILayout.Width(200)))
            {
                OnEnable();
            }
            EditorGUILayout.EndHorizontal();
            if (files != null)
            {


                scroll = EditorGUILayout.BeginScrollView(scroll);
                foreach (var file in files.ToArray())
                {
                    DrawFile(file);
                }
                EditorGUILayout.EndScrollView();
            }
        }
        void DrawFile(FileMeta file)
        {
            EditorGUILayout.BeginHorizontal("HelpBox");
            EditorGUILayout.SelectableLabel(file.name);
            EditorGUILayout.SelectableLabel(file.size == -1 ? "-" : file.size.NormalizeFileSize(), GUILayout.Width(100));
            if (GUILayout.Button("Download", "toolbarbutton", GUILayout.Width(100), GUILayout.Height(20)))
            {
                Download(file);
            }
            if (GUILayout.Button("Delete", "toolbarbutton", GUILayout.Width(100), GUILayout.Height(20)))
            {
                Delete(file.id);
            }
            EditorGUILayout.EndHorizontal();

        }

        async void Download(FileMeta fileMeta)
        {
            var path = EditorUtility.SaveFilePanel("Save as", EditorConsts.PREASSETS_PATH, fileMeta.name, "");
            if (!string.IsNullOrEmpty(path))
            {
                downloadPath = path;
                MainWindow.OnProgressChanged(0, $"Starting download..."); 
                await storageControl.DownloadFile(fileMeta.link, path, fileMeta.size == -1 ? 0 : fileMeta.size);
                OnDownloadComplete();
            }
        }

        async void LoadMeta()
        {
            MainWindow.OnProgressChanged(0, $"Loading...");
            files = await storageControl.GetListFileMeta();
            MainWindow.ResetProgress();
            MainWindow.DoRepaint();
        }

        async void Delete(string id)
        {
            MainWindow.OnProgressChanged(0, $"Process deleting...");
            files.RemoveAll(x => x.id == id);
            MainWindow.ResetProgress();
            await storageControl.DeleteFile(id);
           
        }
      
        void OnDownloadProgressChanged(ProgressData data)
        {
            if (data.targetBytes != 0)
            {
                MainWindow.OnProgressChanged(data.progress,
                    $"Downloading file {(data.downloadedBytes / data.targetBytes).ToString("00")}%...");
            }
            else
            {
                MainWindow.OnProgressChanged(data.progress,
                    $"Downloading file {(data.downloadedBytes.NormalizeFileSize())}...");
            }

            SharedProgressBar.DisplayProgress("Downloading file...", data.progress);
        }
        void OnDownloadComplete()
        {
            MainWindow.ResetProgress();
            SharedProgressBar.ClearProgress();
            EditorDispatcher.Dispatch(() => EditorUtility.RevealInFinder(downloadPath));
        }
    }
}