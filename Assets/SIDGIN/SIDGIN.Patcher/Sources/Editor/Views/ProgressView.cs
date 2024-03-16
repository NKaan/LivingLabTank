using UnityEngine;
using UnityEditor;
namespace SIDGIN.Patcher.Editors
{
    public class ProgressView
    {
        float progress;
        string status;

        public void Progress(string status, float progress)
        {
            this.progress = progress;
            this.status = status;
        }
        public void OnDraw(Rect position)
        {
            var rect = EditorGUILayout.BeginHorizontal();
            rect.width = position.width - 60;
            rect.height = 20;
            EditorGUI.ProgressBar(rect, progress, status);
            rect.x = rect.width;
            rect.width = 60;
            if (GUI.Button(rect, "Cancel"))
            {
                MainWindow.CancelCurrentTask();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("");
        }
    }
}