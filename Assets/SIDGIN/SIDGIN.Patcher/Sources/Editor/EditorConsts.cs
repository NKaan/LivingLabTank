using UnityEngine;
using System.IO;
using SIDGIN.Common.Editors;

namespace SIDGIN.Patcher.Editors
{
    public static class EditorConsts
    {
        public const string MAIN_FOLDER_NAME = "SIDGIN.Patcher";
        public static string PREASSETS_PATH
        {
            get
            {
                return Application.persistentDataPath;
            }
        }
        public static string CACHE_PATH
        {
            get
            {
               return Path.Combine(PREASSETS_PATH, "SGPatcherSources");
            }
        }
        public static string VERSIONS_PATH
        {
            get
            {
                var settingsData = EditorSettingsLoader.Get<SettingsData>();
                string versionssPath;
                if (settingsData.SelectedDefinition != null)
                {
                    versionssPath = Path.Combine(PREASSETS_PATH, "SGPatcherSources/Versions",
                     settingsData.SelectedDefinition.Name);
                }
                else
                {
                    versionssPath = Path.Combine(PREASSETS_PATH, "SGPatcherSources/Versions");
                }
                if (!Directory.Exists(versionssPath))
                {
                    Directory.CreateDirectory(versionssPath);
                }
                return versionssPath;
            }
        }
        public static string TEMP_PATH
        {
            get
            {
                var settingsData = EditorSettingsLoader.Get<SettingsData>();
                string patchesPath;
                if (settingsData.SelectedDefinition != null)
                {
                    patchesPath = Path.Combine(PREASSETS_PATH, "SGPatcherSources/Temp",
                    settingsData.SelectedDefinition.Name);
                }
                else
                {
                    patchesPath = Path.Combine(PREASSETS_PATH, "SGPatcherSources/Temp");
                }
                if (!Directory.Exists(patchesPath))
                {
                    Directory.CreateDirectory(patchesPath);
                }
                return patchesPath;
            }
        }
    }
}
