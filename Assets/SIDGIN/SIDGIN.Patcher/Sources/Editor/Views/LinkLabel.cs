using UnityEditor;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    public static partial class EditorControls
    {
        static GUIStyle m_LinkStyle;
        static GUIStyle LinkStyle
        {
            get
            {
                if (m_LinkStyle == null)
                {
                    m_LinkStyle = new GUIStyle(EditorStyles.label);
                    m_LinkStyle.wordWrap = false;
                    m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
                    m_LinkStyle.stretchWidth = false;
     
                }
                return m_LinkStyle;
            }
        }
        
        public static void LinkLabel(GUIContent label, string url, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

            Handles.BeginGUI();
            Handles.color = LinkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            if(GUI.Button(position, label, LinkStyle))
            {
                Application.OpenURL(url);
            }
        }
        public static void LinkLabel(string label, string url, params GUILayoutOption[] options)
        {
            LinkLabel(new GUIContent(label), url, options);
        }
    }
}
