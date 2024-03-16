using SIDGIN.Patcher.Client;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIDGIN.Patcher.Client
{
    class SceneLoadedData
    {
        public AssetBundle bundle;
        public SceneData scene;
    }
    internal static class SceneLoadManager
    {
        internal static int sceneCountInBuildSettings => runtimeData != null ? runtimeData.sceneDatas.Count : 0;
        static RuntimeData runtimeData;
        static List<AssetBundle> sharedResourcesLoaded = new List<AssetBundle>();
        static List<SceneLoadedData> scenesLoaded = new List<SceneLoadedData>();
        static Dictionary<string, string> gameFiles = new Dictionary<string, string>();
        static SceneLoadManager()
        {
            Initialize();
        }
        public static void Initialize()
        {
            if (runtimeData == null)
            {
                var assetBundle = AssetBundle.LoadFromFile($"{Application.persistentDataPath}/{Consts.MAIN_PACKAGE_NAME}/{Consts.RUNTIME_DATA_NAME}");
                runtimeData = assetBundle.LoadAsset<RuntimeData>("RuntimeData");
            }
            gameFiles = new Dictionary<string, string>();
            var files = Directory.GetFiles(Application.persistentDataPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.Contains("SGPatcherSources"))
                    continue;
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (runtimeData.sceneDatas.Any(x => $"{Consts.SCENE_PREFIX}{x.index}" == fileName) || runtimeData.sharedResources.Contains(fileName))
                {
                    if (!gameFiles.ContainsKey(fileName))
                    {
                        gameFiles.Add(fileName, file);
                    }
                }
            }
        }
        internal static SceneData GetSceneDataByIndex(int index)
        {
            var sceneData = runtimeData.sceneDatas.Find(x => x.index == index);
            if (sceneData == null)
            {
                throw new System.ApplicationException($"Scene with index {index} not found.");
            }
            return sceneData;
        }
        internal static SceneData GetSceneDataByName(string sceneName)
        {
            var sceneData = runtimeData.sceneDatas.Find(x => x.name == sceneName);
            if (sceneData == null)
            {
                throw new System.ApplicationException($"Scene with name {sceneName} not found.");
            }
            return sceneData;
        }

        internal static SceneData GetActiveScene()
        {
            foreach (var sceneLoadedData in scenesLoaded)
            {
                if (sceneLoadedData.scene.IsActive)
                    return sceneLoadedData.scene;
            }

            return null;
        }
       
        #region Load Scene Sync
        internal static void LoadScene(int index, object parameters)
        {
            var sceneData = GetSceneDataByIndex(index);
            LoadScene(sceneData, parameters);
        }
        internal static void LoadScene(string sceneName, object parameters)
        {
            var sceneData = GetSceneDataByName(sceneName);
            LoadScene(sceneData, parameters);
        }
        static void LoadScene(SceneData scene, object parameters)
        {
            if (!string.IsNullOrEmpty(scene.sharedResources) && scene.sharedResources != "None")
            {
                if (sharedResourcesLoaded.All(x => x.name != scene.sharedResources))
                {
                    //Load Shared Resources
                    var assetBundle = LoadAsset(scene.sharedResources);
                    if (assetBundle == null)
                        throw new System.ApplicationException("Unable to load resources for the level.");
                    sharedResourcesLoaded.Add(assetBundle);
                }

            }
            string sceneBundleName = $"{Consts.SCENE_PREFIX}{scene.index}";
            if (scenesLoaded.Any(x => x.scene == scene))
            {
                LoadSceneInternal(scene, parameters);
            }
            else
            {
                var assetBundle = LoadAsset(sceneBundleName);
                if (assetBundle == null)
                    throw new System.ApplicationException("Unable to load level.");
                LoadSceneInternal(scene, parameters);
                scenesLoaded.Add(new SceneLoadedData { scene = scene, bundle = assetBundle });
            }
        }
        static void LoadSceneInternal(SceneData sceneData, object parameters)
        {
            if (parameters is LoadSceneMode sceneMode)
            {
                if (sceneMode == LoadSceneMode.Single)
                    sceneData.IsActive = true;
                SceneManager.LoadScene(sceneData.name, sceneMode);
            }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            else if (parameters is LoadSceneParameters sceneParameters)
            {
                if (sceneParameters.loadSceneMode == LoadSceneMode.Single)
                    sceneData.IsActive = true;
                SceneManager.LoadScene(sceneData.name, sceneParameters);
            }
#endif
            else
            {
                SceneManager.LoadScene(sceneData.name);
            }
        }
        #endregion

        #region Load Scene Async
        internal static SGAsyncOperation LoadSceneAsync(int index, object loadParameters)
        {
            var sceneData = GetSceneDataByIndex(index);
            return LoadSceneAsync(sceneData, loadParameters);
        }
        internal static SGAsyncOperation LoadSceneAsync(string sceneName, object loadParameters)
        {
            var sceneData = GetSceneDataByName(sceneName);
            return LoadSceneAsync(sceneData, loadParameters);
        }

        static SGAsyncOperation LoadSceneAsync(SceneData scene, object loadParameters)
        {
            var asyncOperation = new SGAsyncOperation(op => LoadSceneAsync(scene, op, loadParameters));
            return asyncOperation;
        }

        static IEnumerator LoadSceneAsync(SceneData scene, SGAsyncOperation operation, object loadParameters)
        {
            float deltaProgress = 1f;
            float startProgress = 0;
            if (!string.IsNullOrEmpty(scene.sharedResources) && scene.sharedResources != "None")
            {
                if (sharedResourcesLoaded.All(x => x.name != scene.sharedResources))
                {
                    deltaProgress = 0.5f;
                    var assetResult = LoadAssetAsync(scene.sharedResources, operation);
                    if (assetResult.request != null)
                    {
                        yield return assetResult.request;
                        operation.progress = deltaProgress;
                        if (assetResult.request == null)
                        {
                            operation.isDone = true;
                            throw new System.ApplicationException("Unable to load resources for the level.");
                        }
                        sharedResourcesLoaded.Add(assetResult.request.assetBundle);
                    }
                    else
                    {
                        if (assetResult.bundle != null)
                        {
                            sharedResourcesLoaded.Add(assetResult.bundle);
                        }
                        else
                        {
                            throw new System.ApplicationException("Unable to load resources for the level.");
                        }
                    }
                    startProgress = deltaProgress;
                }
                
            }
            string sceneBundleName = $"{Consts.SCENE_PREFIX}{scene.index}";
            if (scenesLoaded.Any(x => x.scene == scene))
            {
                var asyncOperation = CreateAsyncLoadScene(scene, loadParameters);
                asyncOperation.allowSceneActivation = operation.allowSceneActivation;
                while (!asyncOperation.isDone)
                {
                    operation.progress = startProgress + deltaProgress * asyncOperation.progress;
                    asyncOperation.allowSceneActivation = operation.allowSceneActivation;
                    yield return null;
                }
            }
            else
            {
                deltaProgress *= 0.5f;
                var assetResult = LoadAssetAsync(sceneBundleName, operation);
                AssetBundle assetBundle = null;
                if (assetResult.request != null)
                {
                    yield return assetResult.request;
                    operation.progress = startProgress + deltaProgress * assetResult.request.progress;
                    if (assetResult.request.assetBundle == null)
                    {
                        operation.isDone = true;
                        throw new System.ApplicationException("Unable to load level.");
                    }
                    assetBundle = assetResult.request.assetBundle;
                }
                else
                {
                    if (assetResult.bundle == null)
                    { 
                        throw new System.ApplicationException("Unable to load level.");
                    }
                    assetBundle = assetResult.bundle;
                }

                startProgress += deltaProgress;

                var asyncOperation = CreateAsyncLoadScene(scene, loadParameters);
                asyncOperation.allowSceneActivation = operation.allowSceneActivation;
                while (!asyncOperation.isDone)
                {
                    operation.progress = startProgress + deltaProgress * asyncOperation.progress;
                    asyncOperation.allowSceneActivation = operation.allowSceneActivation;
                    yield return null;
                }
                Debug.Log("Scene is allowed");
                scenesLoaded.Add(new SceneLoadedData { scene = scene, bundle = assetBundle });
            }
        }
        static AsyncOperation CreateAsyncLoadScene(SceneData sceneData, object parameters)
        {
            if (parameters is LoadSceneMode sceneMode)
            {
                if (sceneMode == LoadSceneMode.Single)
                    sceneData.IsActive = true;
                return SceneManager.LoadSceneAsync(sceneData.name, sceneMode);
            }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            else if (parameters is LoadSceneParameters sceneParameters)
            {
                if (sceneParameters.loadSceneMode == LoadSceneMode.Single)
                    sceneData.IsActive = true;
                return SceneManager.LoadSceneAsync(sceneData.name, sceneParameters);
            }
#endif
            else
            {
                return SceneManager.LoadSceneAsync(sceneData.name);
            }
        }
#endregion

#region Load Asset
        static AssetBundle LoadAsset(string bundleName)
        {
            if (!gameFiles.ContainsKey(bundleName))
            {
                throw new System.ApplicationException($"Unable to load resource: {bundleName}. File not found!");
            }
            return AssetBundle.LoadFromFile(gameFiles[bundleName]);
        }
        static  (AssetBundleCreateRequest request,AssetBundle bundle) LoadAssetAsync(string bundleName, SGAsyncOperation operation)
        {
            if (!gameFiles.ContainsKey(bundleName))
            {
                operation.isDone = true;
                throw new System.ApplicationException($"Unable to load resource: {bundleName}. File not found!");
            }
            try
            {
                return (AssetBundle.LoadFromFileAsync(gameFiles[bundleName]), null);
            }
            catch
            {
                var assetBundle = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(x => x.name == bundleName);
                return (null, assetBundle);
            }
        }
#endregion

#region Unload
        internal static SGAsyncOperation UnloadSceneAsync(int sceneBuildIndex, object parameters)
        {
            var sceneData = GetSceneDataByIndex(sceneBuildIndex);
            var asyncOperation = new SGAsyncOperation(op => UnloadScene(sceneData, parameters));
            return asyncOperation;

        }
        internal static SGAsyncOperation UnloadSceneAsync(string sceneName, object parameters)
        {
            var sceneData = GetSceneDataByName(sceneName);
            var asyncOperation = new SGAsyncOperation(op => UnloadScene(sceneData, parameters));
            return asyncOperation;
        }
        internal static SGAsyncOperation UnloadSceneAsync(Scene scene, object parameters)
        {
            var asyncOperation = new SGAsyncOperation(op => UnloadScene(scene, parameters));
            return asyncOperation;
        }
        static IEnumerator UnloadScene(SceneData scene, object parameters)
        {
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            if (parameters is UnloadSceneOptions)
            {
                yield return SceneManager.UnloadSceneAsync(scene.name, (UnloadSceneOptions)parameters);
            }
            else
            {
                yield return SceneManager.UnloadSceneAsync(scene.name);
            }
#else
            yield return SceneManager.UnloadSceneAsync(scene.name);
#endif
            UnloadSceneAssets(scene);
        }
        static IEnumerator UnloadScene(Scene scene, object parameters)
        {
            var sceneData = GetSceneDataByName(scene.name);
            UnloadSceneAssets(sceneData);
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
            if (parameters is UnloadSceneOptions)
            {
                yield return SceneManager.UnloadSceneAsync(scene, (UnloadSceneOptions)parameters);
            }
            else
            {   
                yield return SceneManager.UnloadSceneAsync(scene);
            }
#else
            yield return SceneManager.UnloadSceneAsync(scene);
#endif
        }
        static void UnloadSceneAssets(SceneData scene)
        {
            var sceneBundle = scenesLoaded.Find(x => x.scene == scene);
            if (sceneBundle != null && sceneBundle.bundle != null)
            {
                sceneBundle.bundle.Unload(true);
                scenesLoaded.Remove(sceneBundle);
                UnloadUnusedSharedAssets();
            }
        }
        static void UnloadUnusedSharedAssets()
        {
            foreach (var sharedAsset in sharedResourcesLoaded)
            {
                if (scenesLoaded.All(x => x.scene.sharedResources != sharedAsset.name))
                {
                    sharedAsset.Unload(true);
                    sharedResourcesLoaded.Remove(sharedAsset);
                }
            }
        }


#endregion
    }
}
