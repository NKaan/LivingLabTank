
using System.Reflection;
using System.Threading;
using UnityEditor;
namespace SIDGIN.Common.Editors
{
    public static class AsyncForcePump
    {
        [InitializeOnLoadMethod]
        static void Initialize() => EditorApplication.update += ExecuteContinuations;

        static MethodInfo execMethod;
        static void ExecuteContinuations()
        {
            var context = SynchronizationContext.Current;
            if (execMethod == null)
                execMethod = context.GetType().GetMethod("Exec", BindingFlags.NonPublic | BindingFlags.Instance);
            execMethod?.Invoke(context, null);
        }
    }
}
