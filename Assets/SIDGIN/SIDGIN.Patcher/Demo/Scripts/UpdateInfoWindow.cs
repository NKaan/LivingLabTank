using SIDGIN.Patcher.Client;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace SIDGIN.Patcher.Unity
{
    public class UpdateInfoWindow : MonoBehaviour
    {
        public event Action onOkClick;
        public event Action onCancelClick;
#pragma warning disable 0649
        [SerializeField]
        Text patchSizeText;
        [SerializeField]
        Text releaseNotesText;
        [SerializeField]
        Button okButton;
        [SerializeField]
        Button cancelButton;
#pragma warning restore 0649
        private void Awake()
        {
            okButton.onClick.AddListener(OnOKClick);
            cancelButton.onClick.AddListener(OnCancelClick);
        }

        void OnOKClick()
        {
            onOkClick?.Invoke();
        }
        void OnCancelClick()
        {
            onCancelClick?.Invoke();
        }
        public void SetPatchSize(long patchSize)
        {
            patchSizeText.text = patchSize.NormalizeFileSize();
        }
        public void SetReleaseNotes(string releaseNotes)
        {
            releaseNotesText.text = releaseNotes;
        }
    }
}