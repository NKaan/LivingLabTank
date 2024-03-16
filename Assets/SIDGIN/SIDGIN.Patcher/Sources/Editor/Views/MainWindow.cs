using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

namespace SIDGIN.Patcher.Editors
{
    using Common;
    using SIDGIN.Common.Editors;
    using SIDGIN.Common.Editors.Controls;
    using SIDGIN.Patcher.Editors.Storages;
    using SIDGIN.Patcher.Storages;

    public class MainWindow : EditorWindow
    {
        enum ViewType
        {
            Build,
            Settings,
            Storage,
            Advanced,
            UploadHistory,
        }
        public static void ShowWindow()
        {
            GetWindow<MainWindow>(true, "SG Patcher");
        }
        static bool isBusy;
        public static bool IsBusy { get { return isBusy; } set { isBusy = value; DoRepaint(); } }

        static MainWindow instance;
        SettingsData settingsData;
        BuildSettingsCacheData buildSettingsCache;
        private void OnEnable()
        {
            instance = this;
            ErrorHandler.onErrorHandled += ErrorHandler_onErrorHandled;
            InternalErrorHandler.onErrorHandled += InternalErrorHandler_onErrorHandled;
            settingsData = EditorSettingsLoader.Get<SettingsData>();
            buildSettingsCache = EditorSettingsLoader.Get<BuildSettingsCacheData>();
            OnEnabledContent();
        }
        private void OnDisable()
        {
            ErrorHandler.onErrorHandled -= ErrorHandler_onErrorHandled;
            EditorSettingsLoader.Get<GoogleSettingsData>().Save();
            EditorSettingsLoader.Get<HttpSettingsData>().Save();
            EditorSettingsLoader.Get<AmazonSettingsData>().Save();
            EditorSettingsLoader.Get<BuildSettingsCacheData>().Save();
            EditorSettingsLoader.Get<SettingsData>().Save();
        }
        private void ErrorHandler_onErrorHandled(System.Exception ex)
        {
            if(!(ex is TaskCanceledException))
            {
                EditorDispatcher.Dispatch(() =>
                {
                    EditorUtility.DisplayDialog("Error", "An error occurred during execution. See the logs for details.", "OK");
                    ResetProgress();
                });
            }
        }
        private void InternalErrorHandler_onErrorHandled(System.Exception ex)
        {
            if (!(ex is TaskCanceledException))
            {
                EditorDispatcher.Dispatch(() =>
                {
                    if(ex is UnableLoadResource) 
                    {
                        Debug.LogWarning(ex);
                    }
                    else
                    {
                        Debug.LogError(ex);
                    }
                    ResetProgress();
               
                });
            }
        }
        public static void CancelCurrentTask()
        {
            CancelationHelper.Cancel();
            ResetProgress();
        }
        public static void DoRepaint()
        {
            if (instance != null)
                instance.Repaint();
        }
        ViewType selectedView;
        BuildView buildView = new BuildView();
        ProgressView progressView = new ProgressView();
        SettingsView settingsView = new SettingsView();
        StorageView storageView = new StorageView();
        UploadHistoryView uploadHistoryView = new UploadHistoryView();
        AdvancedView advancedView = new AdvancedView();
        private void OnGUI()
        {
            var rect = EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            LoadingControl.Begin(IsBusy);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("AnimationEventBackground", GUILayout.Width(150));
            DrawViewSelector("Versions", ViewType.Build);
            DrawViewSelector("Settings", ViewType.Settings);
            DrawViewSelector("Storage", ViewType.Storage);
            DrawViewSelector("Advanced", ViewType.Advanced);

            if (buildSettingsCache != null) 
            {
                var buildSettings = buildSettingsCache.GetSelectedData();
                if (buildSettings != null && buildSettings.packageUploadTasks != null && buildSettings.packageUploadTasks.Count > 0)
                {
                    DrawViewSelector("Aborted uploads", ViewType.UploadHistory);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            DrawContent();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            LoadingControl.End(rect, IsBusy);

            progressView.OnDraw(position);
            EditorGUILayout.EndVertical();
            if (IsBusy)
            {
                Repaint();
            }

        }
        public static void OnProgressChanged(float p, string s)
        {
            isBusy = true;
            if (instance == null)
                return;
            instance.progressView.Progress(s, p);
            EditorDispatcher.Dispatch(instance.Repaint);
        }
        //public static void OnSharedProgressChanged(float progress, string status, string shortStatus)
        //{
        //    isBusy = true;
        //    if (instance == null)
        //        return;
        //    instance.progressView.Progress(status, progress);
        //    SharedProgressBar.DisplayProgress(shortStatus, progress);
        //    EditorDispatcher.Dispatch(instance.Repaint);
        //}

        public static void ResetProgress()
        {
            isBusy = false;
            if (instance == null)
                return;
            instance.progressView.Progress("", 0);
            SharedProgressBar.ClearProgress();
            EditorDispatcher.Dispatch(instance.Repaint);
        }
        public static void ShowHome()
        {
            if (instance == null)
                return;
            instance.selectedView = ViewType.Build;
            instance.OnEnabledContent();
        }

        void DrawViewSelector(string name, ViewType type)
        {
            EditorGUI.BeginChangeCheck();
            if (GUILayout.Toggle(selectedView == type, name, "toolbarbutton"))
            {
                selectedView = type;
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorSettingsLoader.Get<GoogleSettingsData>().Save();
                EditorSettingsLoader.Get<HttpSettingsData>().Save();
                EditorSettingsLoader.Get<AmazonSettingsData>().Save();
                EditorSettingsLoader.Get<BuildSettingsCacheData>().Save();
                /*
                EditorSettingsLoader.Get<UploadHistoryData>().Save();
                */
                settingsData.Save();
                OnEnabledContent();
            }

        }
       
        void DrawContent()
        {
            switch (selectedView)
            {

                case ViewType.Build:
                    buildView.OnDraw();
                    break;
                case ViewType.Settings:
                    settingsView.OnDraw();
                    break;
                case ViewType.Storage:
                    storageView.OnDraw();
                    break;
                case ViewType.Advanced:
                    advancedView.OnDraw();
                    break;
                case ViewType.UploadHistory:
                    uploadHistoryView.OnDraw();
                    break;
            }

        }
        void OnEnabledContent()
        {
            switch (selectedView)
            {

                case ViewType.Build:
                    buildView.OnEnable();
                    break;
                case ViewType.Settings:
                    settingsView.OnEnable();
                    break;
                case ViewType.Storage:
                    storageView.OnEnable();
                    break;
                case ViewType.Advanced:
                    advancedView.OnEnable();
                    break;
                case ViewType.UploadHistory:
                    uploadHistoryView.OnEnable();
                    break;
            }

        }
    }
}