using SIDGIN.Patcher.Client;
using SIDGIN.Patcher.Common;
using SIDGIN.Patcher.SceneManagment;
using SIDGIN.Patcher.Storages;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Version = SIDGIN.Patcher.Client.Version;

namespace SIDGIN.Patcher.Unity
{
    public class ErrorMessage
    {
        public string message;
        public System.Exception exception;
    }
    [System.Serializable]
    public class SGPatcherProgressEvent : UnityEvent<PatcherProgress> { }
    [System.Serializable]
    public class SGPatcherErrorEvent : UnityEvent<ErrorMessage> { }
    public class SGPatcherLoader : MonoBehaviour
    {

        public SGPatcherProgressEvent onProgressChanged;
        public SGPatcherErrorEvent onError;
        public SGPatcherErrorEvent onInternalError;
        public bool startUpdateOnAwake = true;
        public string languageKey;

        CancellationTokenSource tokenSource;
        void Start()
        {
            PrepareLocalizationSheets();
            if (startUpdateOnAwake)
            {
                UpdateGame();
            }
        }
        void PrepareLocalizationSheets()
        {
            var localizationFolder = Path.Combine(Application.persistentDataPath, "loc");
            var languageResources = Resources.LoadAll<TextAsset>("loc/");
            if (languageResources != null && languageResources.Any())
            {
                if (!Directory.Exists(localizationFolder))
                    Directory.CreateDirectory(localizationFolder);
                foreach (var languageData in languageResources)
                {
                    var languageFile = Path.Combine(localizationFolder, $"{languageData.name}.xml");
                    File.WriteAllText(languageFile, languageData.text);
                }
                Localize.Load();
            }
        }
        private static string GetSystemCulture()
        {
            var systemLanguage = Application.systemLanguage;
            var cultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures).
                FirstOrDefault(x => x.EnglishName == Enum.GetName(systemLanguage.GetType(), systemLanguage));
            if (cultureInfo != null)
            {
                return cultureInfo.EnglishName;
            }
            return "";
        }
        PatcherClient PrepareClient()
        {
            var targetPath = Application.persistentDataPath;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            var patcherClinetSettings = ClientSettings.Get();

            InternalErrorHandler.onErrorHandled += InternalErrorHandler_onErrorHandled;
            var localizationFolder = Path.Combine(Application.persistentDataPath, "loc");
            if (System.IO.Directory.Exists(localizationFolder))
            {
                var clientSettings = ClientSettings.Get();
                if (!string.IsNullOrEmpty(languageKey))
                {
                    clientSettings.languageKey = languageKey;
                }
                else
                {
                    clientSettings.languageKey = GetSystemCulture();
                }
            }
            var client = new PatcherClient();
            client.onProgressChanged += OnProgressChanged;
            return client;
        }

        IEnumerator LoadGameAsync()
        {
#if !UNITY_EDITOR
            try
            {
               SGResources.Initialize();
            }
            catch (System.Exception ex)
            {
                onError.Invoke(new ErrorMessage
                        {
                            exception = ex,
                            message = ex + "\n" + ex.InnerException?.Message + "\n" + ex.StackTrace
                        });
                throw ex;
            }
#endif
            var asyncOperation = SGSceneManager.LoadSceneAsync(0);
            while (!asyncOperation.isDone)
            {
                onProgressChanged.Invoke(new PatcherProgress { progress = asyncOperation.progress, status = "Loading..." });
                if (asyncOperation.progress > 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }
                yield return null;
            }
        }
        public void LoadGame()
        {
            if (SGPatcher.Version == Version.Empty)
            {
                UpdateGame();
            }
            else
            {
                StartCoroutine(LoadGameAsync());
            }
        }
        public async Task InstallPackage(string packageName)
        {
            try
            {
                var patcherClient = PrepareClient();
                await patcherClient.InstallPackage(packageName);
                SGSceneManager.Initialize();
                SGResources.Initialize();
            }
            catch (System.Exception ex)
            {
                onError.Invoke(new ErrorMessage
                {
                    exception = ex,
                    message = ex + "\n" + ex.InnerException?.Message + "\n" + ex.StackTrace
                });
            }

        }
        public async Task<UpdateMetaData> GetUpdateMetaData()
        {
            try
            {
                var patcherClient = PrepareClient();
                var metaData = patcherClient.GetUpdateMetaData();
                return await metaData;
            }
            catch (System.Exception ex)
            {
                var patcherClinetSettings = ClientSettings.Get();
                if (patcherClinetSettings.offlineMode)
                {
                    if (ex is UnableConnectToServer || ex is UnableLoadResource)
                    {
                        if (SGPatcher.Version != Version.Empty)
                        {
                            LoadGame();
                        }
                        InternalErrorHandler_onErrorHandled(ex);
                    }
                    else
                    {
                        onError.Invoke(new ErrorMessage
                        {
                            exception = ex,
                            message = ex + "\n" + ex.InnerException?.Message + "\n" + ex.StackTrace
                        });
                      
                    }
                }
                else
                {
                    onError.Invoke(new ErrorMessage
                    {
                        exception = ex,
                        message = ex + "\n" + ex.InnerException?.Message + "\n" + ex.StackTrace
                    });
                }
                return null;
            } 
        }
        public async void UpdateGame()
        {
            var patcherClinetSettings = ClientSettings.Get();

            try
            {
                var client = PrepareClient();
                tokenSource = new CancellationTokenSource();
                await client.Update();
                StartCoroutine(LoadGameAsync());

            }
            catch (Exception ex)
            {
                if (patcherClinetSettings.offlineMode)
                {
                    if (ex is UnableConnectToServer || ex is UnableLoadResource)
                    {
                        if (SGPatcher.Version != Version.Empty)
                        {
                            StartCoroutine(LoadGameAsync());
                        }
                        InternalErrorHandler_onErrorHandled(ex);
                    }
                    else
                    {
                        onError.Invoke(new ErrorMessage
                        {
                            exception = ex,
                            message = ex + "\n" + ex.InnerException?.Message + "\n" + ex.StackTrace
                        });
                    }
                }
                else
                {
                    onError.Invoke(new ErrorMessage
                    {
                        exception = ex,
                        message = ex + "\n" + ex.InnerException?.Message + "\n" + ex.StackTrace
                    });
                }
            }
            finally
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
        }
        private void OnApplicationQuit()
        {
            if (tokenSource != null)
                tokenSource.Cancel();
        }
        private void InternalErrorHandler_onErrorHandled(Exception ex)
        {
            SGDispatcher.Register(() =>
            {
                onInternalError.Invoke(new ErrorMessage
                {
                    exception = ex,
                    message = ex + "\n" + ex.InnerException?.Message + "\n" + ex.StackTrace
                });
            });
        }

        private void OnProgressChanged(PatcherProgress patcherProgress)
        {
            SGDispatcher.Register(() =>
            {
                var progress = patcherProgress;
                onProgressChanged.Invoke(progress);
            });
        }


    }
}
