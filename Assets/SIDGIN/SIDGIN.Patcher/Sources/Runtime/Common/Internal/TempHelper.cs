namespace SIDGIN.Patcher.Common
{
    using System.IO;
    using System;

    internal static class TempHelper
    {
        public static string GetTempFolder()
        {
            return UnityEngine.Application.persistentDataPath;
        }
        public static string GetTempFileName()
        {
            return Path.Combine(UnityEngine.Application.persistentDataPath, $"{Guid.NewGuid().ToString()}.tmp");
        }
    }
}
