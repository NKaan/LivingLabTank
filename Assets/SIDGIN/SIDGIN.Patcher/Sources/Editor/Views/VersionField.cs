using UnityEditor;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    using SIDGIN.Patcher.Client;

    public static class VersionField
    {
        public static Version Show(Version version)
        {
            string labelStyle = "IN TitleText";
            int width = 50;
            EditorGUILayout.BeginHorizontal();
        
            version.Major = EditorGUILayout.IntField(version.Major, GUILayout.Width(width));
            GUILayout.Label(".", labelStyle, GUILayout.Width(1));
            version.Minor = EditorGUILayout.IntField(version.Minor, GUILayout.Width(width));
            GUILayout.Label(".", labelStyle, GUILayout.Width(1));
            version.Build = EditorGUILayout.IntField(version.Build, GUILayout.Width(width));
            GUILayout.Label(".", labelStyle, GUILayout.Width(1));
            version.Revision = EditorGUILayout.IntField(version.Revision,GUILayout.Width(width));
           
            EditorGUILayout.EndHorizontal();
          
            return version;
        }
    }
}
