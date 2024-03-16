using System;
using System.IO;
using SIDGIN.Patcher.Common;
using UnityEngine;
namespace SIDGIN.Patcher.Client
{
    public static class SGPatcher
    {
        public static Version Version
        {
            get
            {
                var appData = GetApplicationData();
                if (appData == null)
                {
                    return Version.Empty;
                }
                else
                {
                    return appData.Version;
                }
            }
        }
        
        
        static ApplicationData GetApplicationData()
        {
            var applicationDataPath = Path.Combine(Application.persistentDataPath, Consts.APP_DATA_FILE_NAME);
            if (File.Exists(applicationDataPath))
            {
                try
                {
                    var applicationDataStr = File.ReadAllText(applicationDataPath);
                    var applicationData = XmlSerializeHelper.Deserialize<ApplicationData>(applicationDataStr);

                    if (applicationData != null)
                    {
                        return applicationData;
                    }
                    else
                    {
                        File.Delete(applicationDataPath);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    //Ошибка чтения
                    Debug.LogError($"[SG Patcher] - Error reading {Consts.APP_DATA_FILE_NAME} file: " + ex);
                    File.Delete(applicationDataPath);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        
    }
}
