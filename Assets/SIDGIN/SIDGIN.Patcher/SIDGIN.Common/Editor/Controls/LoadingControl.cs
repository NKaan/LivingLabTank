using UnityEngine;
using UnityEditor;

namespace SIDGIN.Common.Editors.Controls
{
    public static class LoadingControl
    {

        static LoadingControl()
        {
            EditorApplication.update += Update;
        }
        static Color[] colors = new Color[]
        {
            new Color(0.37647f,0.37647f,0.37647f,0.5f),
            new Color(0.48235f,0.48235f,0.48235f,0.5f),
            new Color(0.57647f,0.57647f,0.57647f,0.5f),
            new Color(0.65882f,0.65882f,0.65882f,0.5f),
            new Color(0.70588f,0.70588f,0.70588f,0.5f),
            new Color(0.78431f,0.78431f,0.78431f,0.5f),
        };
        static float size = 10f;
        static int countRects = 5;
        static double lastTimeSaved;
        static int indexProgress;
        public static void Begin(bool isShow)
        {
            EditorGUI.BeginDisabledGroup(isShow);
         
        }

        public static void End(Rect position, bool isShow)
        {
            if (isShow)
            {
                EditorGUI.DrawRect(position, new Color(0f, 0f, 0f, 0.2f));
                float halfControlSize = (size + 5) * 6 * 0.5f;
                float xPos = position.x + position.width * 0.5f - halfControlSize;
                float yPos = position.y + position.height * 0.5f - size * 0.5f;
                for (int i = 0; i < countRects; i++)
                {
                    EditorGUI.DrawRect(new Rect(xPos + (size + 5) * i, yPos, size, size), GetColor(i));
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        static void Update()
        {
            indexProgress = (int)((EditorApplication.timeSinceStartup - lastTimeSaved)*5f); 
            if (indexProgress >= countRects)
            {
                indexProgress = 0;
                lastTimeSaved = EditorApplication.timeSinceStartup;
            }
        }
        static Color GetColor(int index)
        {
            if (index > indexProgress)
            {
                return colors[colors.Length - (index - indexProgress)];
            }
            else
            {
                return colors[indexProgress - index];
            }
        }
    }
}