using SIDGIN.Patcher.Client;
using SIDGIN.Patcher.Unity;
using UnityEngine;
using UnityEngine.UI;
namespace SIDGIN.Patcher.Unity
{
    public class SGPatcherLoaderView : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        Image progressBar;
        [SerializeField]
        Text progressText;
        [SerializeField]
        Text versionText;
        [SerializeField]
        ErrorWindow errorWindow;
#pragma warning restore 0649

        float mainProgress;
        float deltaProgress;
        void Start()
        {
            var version = SGPatcher.Version;
            if (version != Version.Empty)
            {
                versionText.text = version.ToString();
            }
            else
            {
                versionText.text = "";
            }
        }
        public void OnError(ErrorMessage message)
        {
            errorWindow.Show(message.message);
            Debug.LogError(message.exception);
            Debug.LogError(message.exception?.InnerException);
        }
        public void OnInternalError(ErrorMessage message)
        {
            Debug.LogError(message.exception);
            Debug.LogError(message.exception?.InnerException);
        }
        public void OnProgressChanged(PatcherProgress patcherProgress)
        {
            progressBar.fillAmount = patcherProgress.progress;
            progressText.text = patcherProgress.status;
        }
    }
}