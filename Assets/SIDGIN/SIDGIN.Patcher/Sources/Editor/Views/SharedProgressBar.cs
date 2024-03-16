using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
namespace SIDGIN.Patcher.Editors
{
    public static class SharedProgressBar
    {
        private static MethodInfo m_display = null;
        private static MethodInfo m_clear = null;

        static SharedProgressBar()
        {
            //var type = typeof(Editor).Assembly
            //    .GetTypes()
            //    .Where(c => c.Name == "AsyncProgressBar")
            //    .FirstOrDefault()
            //;

            //m_display = type.GetMethod("Display");
            //m_clear = type.GetMethod("Clear");
        }



        public static void DisplayProgress(string label, float progress)
        {
            //EditorDispatcher.Dispatch(() => _DisplayProgress(label, progress));
        }
        public static void _DisplayProgress(string label, float progress)
        {
            //try
            //{
            //    var parameters = new object[] { label, progress };
            //    m_display.Invoke(null, parameters);
            //}
            //catch (Exception ex)
            //{

            //}
        }
        public static void ClearProgress()
        {
            //EditorDispatcher.Dispatch(ClearProgress);
        }
        public static void _ClearProgress()
        {
            try
            {
                m_clear.Invoke(null, null);
                EditorUtility.ClearProgressBar();
            }
            catch (Exception ex)
            {

            }
        }
    }
}