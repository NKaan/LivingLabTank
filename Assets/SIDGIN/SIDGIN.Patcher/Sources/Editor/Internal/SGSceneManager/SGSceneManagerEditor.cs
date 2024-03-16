using SIDGIN.Common.Editors;
using SIDGIN.Patcher.Client;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace SIDGIN.Patcher.Editors
{
    public class SGSceneManagerEditor
    {
        static RuntimeData runtimeData;
        static BuildSettingsData settings;
        static SceneEditorData activeScene;
        static SGSceneManagerEditor()
        {
            runtimeData = EditorSettingsLoader.Get<RuntimeData>();
            settings = EditorSettingsLoader.Get<BuildSettingsCacheData>().GetSelectedData();
        }
        public static int sceneCountInSettings => runtimeData != null ? runtimeData.sceneDatas.Count : 0;


        public static SceneData GetSceneByBuildIndex(int index)
        {
            var sceneData = runtimeData.sceneDatas.Find(x => x.index == index);
            if (sceneData == null)
            {
                throw new System.ApplicationException($"Scene with index {index} not found.");
            }
            return sceneData;
        }
        public static SceneData GetSceneByName(string sceneName)
        {
            var sceneData = runtimeData.sceneDatas.Find(x => x.name == sceneName);
            if (sceneData == null)
            {
                throw new System.ApplicationException($"Scene with name {sceneName} not found.");
            }
            return sceneData;
        }
        static SceneEditorData GetSceneDataByName(string sceneName)
        {
            var sceneData = settings.scenes.Find(x => x.name == sceneName);
            if (sceneData == null)
            {
                throw new System.ApplicationException($"Scene with name {sceneName} not found.");
            }
            return sceneData;
        }
        static SceneEditorData GetSceneDataByIndex(int index)
        {
            var sceneData = settings.scenes.Find(x => x.index == index);
            if (sceneData == null)
            {
                throw new System.ApplicationException($"Scene with index {index} not found.");
            }
            return sceneData;
        }
        #region Load Scene Sync

        public static void LoadScene(string sceneName, LoadSceneMode mode)
        {
            var sceneData = GetSceneDataByName(sceneName);
            if (mode == LoadSceneMode.Single)
                activeScene = sceneData;
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            EditorSceneManager.LoadSceneInPlayMode(sceneData.path, new LoadSceneParameters(mode));
#else
            EditorSceneManager.LoadScene(sceneData.path, mode);
#endif
        }
        public static void LoadScene(string sceneName)
        {
            var sceneData = GetSceneDataByName(sceneName); 
            activeScene = sceneData;
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            EditorSceneManager.LoadSceneInPlayMode(sceneData.path, new LoadSceneParameters());
#else
            EditorSceneManager.LoadScene(sceneData.path);
#endif
        }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static Scene LoadScene(string sceneName, LoadSceneParameters parameters)
        {
            var sceneData = GetSceneDataByName(sceneName);
            if (parameters.loadSceneMode == LoadSceneMode.Single)
                activeScene = sceneData;
            EditorSceneManager.LoadSceneInPlayMode(sceneData.path, parameters);
            return SceneManager.GetSceneByName(sceneName);
        }
#endif
        public static void LoadScene(int sceneBuildIndex, LoadSceneMode mode)
        {
            var sceneData = GetSceneDataByIndex(sceneBuildIndex);
            if (mode == LoadSceneMode.Single)
                activeScene = sceneData;
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            EditorSceneManager.LoadSceneInPlayMode(sceneData.path, new LoadSceneParameters(mode));
#else
            EditorSceneManager.LoadScene(sceneData.path);
#endif
        }
        public static void LoadScene(int sceneBuildIndex)
        {
            var sceneData = GetSceneDataByIndex(sceneBuildIndex);
            activeScene = sceneData;
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            EditorSceneManager.LoadSceneInPlayMode(sceneData.path, new LoadSceneParameters());
#else
            EditorSceneManager.LoadScene(sceneData.path);
#endif
        }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static Scene LoadScene(int sceneBuildIndex, LoadSceneParameters parameters)
        {
            var sceneData = GetSceneDataByIndex(sceneBuildIndex);
            if (parameters.loadSceneMode == LoadSceneMode.Single)
                activeScene = sceneData;
            EditorSceneManager.LoadSceneInPlayMode(sceneData.path, parameters);
            return SceneManager.GetSceneByName(sceneData.name);
        }
#endif
        #endregion

        #region Load Scene Async
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static SGAsyncOperation LoadSceneAsync(string sceneName, LoadSceneParameters parameters)
        {
            var sceneData = GetSceneDataByName(sceneName);
            if (parameters.loadSceneMode == LoadSceneMode.Single)
                activeScene = sceneData;
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op, parameters));
            return asyncOperation;
        }
#endif
        public static SGAsyncOperation LoadSceneAsync(string sceneName)
        {
            var sceneData = GetSceneDataByName(sceneName);
            activeScene = sceneData;
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op, new LoadSceneParameters()));
#else
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op));
#endif
            return asyncOperation;
        }
        public static SGAsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            var sceneData = GetSceneDataByName(sceneName);
            if (mode == LoadSceneMode.Single)
                activeScene = sceneData;
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op, new LoadSceneParameters(mode)));
#else
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op, mode));
#endif
            return asyncOperation;
        }
        public static SGAsyncOperation LoadSceneAsync(int sceneBuildIndex)
        {
            var sceneData = GetSceneDataByIndex(sceneBuildIndex);
            activeScene = sceneData;
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op, new LoadSceneParameters()));
#else
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op));
#endif
            return asyncOperation;
        }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static SGAsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneParameters parameters)
        {
            var sceneData = GetSceneDataByIndex(sceneBuildIndex);
            if (parameters.loadSceneMode == LoadSceneMode.Single)
                activeScene = sceneData;
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op, parameters));
            return asyncOperation;
        }
#endif
        public static SGAsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode)
        {
            var sceneData = GetSceneDataByIndex(sceneBuildIndex);
            if (mode == LoadSceneMode.Single)
                activeScene = sceneData;
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op, new LoadSceneParameters(mode)));
#else
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(sceneData, op, mode));
#endif
            return asyncOperation;
        }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        static IEnumerator LoadSceneAsync(SceneEditorData scene, SGAsyncOperation operation, LoadSceneParameters loadParameters)
        {
                var asyncOperation = EditorSceneManager.LoadSceneAsyncInPlayMode (scene.path, loadParameters);
                asyncOperation.allowSceneActivation = operation.allowSceneActivation;
                while (!asyncOperation.isDone)
                {
                    operation.progress = asyncOperation.progress;
                    yield return null;
                }
                while (!operation.allowSceneActivation)
                {
                    yield return null;
                }
                asyncOperation.allowSceneActivation = true;
        }
#else
        static IEnumerator LoadSceneAsync(SceneEditorData scene, SGAsyncOperation operation, LoadSceneMode mode = default)
        {
            var asyncOperation = EditorSceneManager.LoadSceneAsync(scene.path, mode);
            asyncOperation.allowSceneActivation = operation.allowSceneActivation;
            while (!asyncOperation.isDone)
            {
                operation.progress = asyncOperation.progress;
                yield return null;
            }
            while (!operation.allowSceneActivation)
            {
                yield return null;
            }
            asyncOperation.allowSceneActivation = true;
        }
#endif

#endregion



#region Unload
        public static SGAsyncOperation UnloadSceneAsync(int sceneBuildIndex)
        {
            var sceneData = GetSceneDataByIndex(sceneBuildIndex);
            if (sceneData == activeScene)
                activeScene = null;
            var asyncOperation = new SGAsyncOperation(op => UnloadScene(sceneData.name, null));
            return asyncOperation;
        }
        public static SGAsyncOperation UnloadSceneAsync(string sceneName)
        { 
            var asyncOperation = new SGAsyncOperation(op => UnloadScene(sceneName, null));
            return asyncOperation;
        }
        public static SGAsyncOperation UnloadSceneAsync(Scene scene)
        {
            var asyncOperation = new SGAsyncOperation(op => UnloadScene(scene.name, null));
            return asyncOperation;
        }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static SGAsyncOperation UnloadSceneAsync(int sceneBuildIndex, UnloadSceneOptions options)
        {
            var sceneData = GetSceneDataByIndex(sceneBuildIndex);
            if (sceneData == activeScene)
                activeScene = null;
            var asyncOperation = new SGAsyncOperation(op => UnloadScene(sceneData.name, options));
            return asyncOperation;
        }
        public static SGAsyncOperation UnloadSceneAsync(string sceneName, UnloadSceneOptions options)
        {
            var asyncOperation = new SGAsyncOperation(op => UnloadScene(sceneName, options));
            return asyncOperation;
        }
        public static SGAsyncOperation UnloadSceneAsync(Scene scene, UnloadSceneOptions options)
        {
            var asyncOperation = new SGAsyncOperation(op => UnloadScene(scene.name, options));
            return asyncOperation;
        }
#endif
        static IEnumerator UnloadScene(string sceneName, object parameters)
        {
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            if (parameters is UnloadSceneOptions)
            {
                yield return SceneManager.UnloadSceneAsync(sceneName, (UnloadSceneOptions)parameters);
            }
            else
            {
                yield return SceneManager.UnloadSceneAsync(sceneName);
            }
#else
            yield return SceneManager.UnloadSceneAsync(sceneName);
#endif
        }
#endregion
    }
}
