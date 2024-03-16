using SIDGIN.Patcher.Client;
using UnityEngine.SceneManagement;

namespace SIDGIN.Patcher.SceneManagment
{
    public class InternalSGSceneManager
    {
        public static int sceneCountInSettings => SceneLoadManager.sceneCountInBuildSettings;

        public static void Initialize()
        {
            SceneLoadManager.Initialize();
        }
        public static SceneData GetSceneByBuildIndex(int buildIndex)
        {
            return SceneLoadManager.GetSceneDataByIndex(buildIndex);
        }

        public static SceneData GetSceneByName(string sceneName)
        {
            return SceneLoadManager.GetSceneDataByName(sceneName);
        }
        
        public static SceneData GetActiveScene()
        {
            var sceneData = SceneLoadManager.GetActiveScene();
            return sceneData;
        }
        #region Load Scene Sync
        public static void LoadScene(string sceneName, LoadSceneMode mode)
        {
            SceneLoadManager.LoadScene(sceneName,mode);
        }
        public static void LoadScene(string sceneName)
        {
            SceneLoadManager.LoadScene(sceneName, null);
        }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static Scene LoadScene(string sceneName, LoadSceneParameters parameters)
        {
            SceneLoadManager.LoadScene(sceneName, parameters);
            return SceneManager.GetSceneByName(sceneName);
        }
#endif
        public static void LoadScene(int sceneBuildIndex, LoadSceneMode mode)
        {
            SceneLoadManager.LoadScene(sceneBuildIndex, mode);
        }
        public static void LoadScene(int sceneBuildIndex)
        {
            SceneLoadManager.LoadScene(sceneBuildIndex, null);
        }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static Scene LoadScene(int sceneBuildIndex, LoadSceneParameters parameters)
        {
            SceneLoadManager.LoadScene(sceneBuildIndex, parameters);
            var sceneData = GetSceneByBuildIndex(sceneBuildIndex);
            return SceneManager.GetSceneByName(sceneData.name);
        }
#endif
        #endregion

        #region Load Scene Async
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static SGAsyncOperation LoadSceneAsync(string sceneName, LoadSceneParameters parameters)
        {
            return SceneLoadManager.LoadSceneAsync(sceneName, parameters);
        }
#endif
        public static SGAsyncOperation LoadSceneAsync(string sceneName)
        {
            return SceneLoadManager.LoadSceneAsync(sceneName, null);
        }
        public static SGAsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            return SceneLoadManager.LoadSceneAsync(sceneName, mode);
        }
        public static SGAsyncOperation LoadSceneAsync(int sceneBuildIndex)
        {
            return SceneLoadManager.LoadSceneAsync(sceneBuildIndex, null);
        }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static SGAsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneParameters parameters)
        {
            return SceneLoadManager.LoadSceneAsync(sceneBuildIndex, parameters);
        }
#endif
        public static SGAsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode)
        {
            return SceneLoadManager.LoadSceneAsync(sceneBuildIndex, mode);
        }
#endregion

#region Unload
        public static SGAsyncOperation UnloadSceneAsync(int sceneBuildIndex)
        {
            return SceneLoadManager.UnloadSceneAsync(sceneBuildIndex, null);
        }
        public static SGAsyncOperation UnloadSceneAsync(string sceneName)
        {
            return SceneLoadManager.UnloadSceneAsync(sceneName, null);
        }
        public static SGAsyncOperation UnloadSceneAsync(Scene scene)
        {
            return SceneLoadManager.UnloadSceneAsync(scene, null);
        }
#if UNITY_2018_4_OR_NEWER && !SIDGIN_UNITY_2018_3
        public static SGAsyncOperation UnloadSceneAsync(int sceneBuildIndex, UnloadSceneOptions options)
        {
            return SceneLoadManager.UnloadSceneAsync(sceneBuildIndex, options);
        }
        public static SGAsyncOperation UnloadSceneAsync(string sceneName, UnloadSceneOptions options)
        {
            return SceneLoadManager.UnloadSceneAsync(sceneName, options);
        }
        public static SGAsyncOperation UnloadSceneAsync(Scene scene, UnloadSceneOptions options)
        {
            return SceneLoadManager.UnloadSceneAsync(scene, options);
        }
#endif
#endregion
    }
}
