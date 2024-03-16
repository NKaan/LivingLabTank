
using SIDGIN.Common.Editors;
using UnityEditor;
#if SGPATCHER_GOOGLE
namespace SIDGIN.Patcher.Editors.Storages
{
    public class GoogleSettingsView : StorageSettingsView
    {
        public override void OnDraw()
        {
            var googleSettingsData = EditorSettingsLoader.Get<GoogleSettingsData>();
            googleSettingsData.sharedFolder = EditorGUILayout.TextField("Shared Folder:", googleSettingsData.sharedFolder);

        }

        public override void OnEnable()
        {

        }
    }
}
#endif