using SIDGIN.Patcher.Unity;
using UnityEngine;
namespace SIDGIN.Patcher.Unity
{
    public class SGPatcherControl : MonoBehaviour
    {
        SGPatcherLoader loader;
#pragma warning disable 0649
        [SerializeField]
        UpdateInfoWindow updateInfoWindow;
        [SerializeField]
        RequiredUpdateMainBuildView requiredUpdateMainBuildWindow;
#pragma warning restore 0649
        private void Awake()
        {
            loader = GetComponent<SGPatcherLoader>();
        }
        async void Start()
        {
            var updateData = await loader.GetUpdateMetaData();
            if (updateData == null)
                return;
            if (updateData.updateStatus == SIDGIN.Patcher.Client.UpdateStatus.Main_Build_Update_Required)
            {
                requiredUpdateMainBuildWindow.gameObject.SetActive(true);
            }
            else if (updateData.updateStatus == SIDGIN.Patcher.Client.UpdateStatus.Required)
            {
                updateInfoWindow.onOkClick += () =>
                {
                    updateInfoWindow.gameObject.SetActive(false);
                    loader.UpdateGame();


                };
                updateInfoWindow.onCancelClick += () =>
                {
                    updateInfoWindow.gameObject.SetActive(false);
                    Application.Quit();
                };
                updateInfoWindow.SetPatchSize(updateData.updateSize);
                updateInfoWindow.SetReleaseNotes(updateData.notes);
                updateInfoWindow.gameObject.SetActive(true);
            }
            else
            {
                loader.LoadGame();
            }

        }
    }
}