using System.Collections.Generic;

namespace SIDGIN.Patcher.Client
{
    public static class LocalizationDefault 
    {
        public static Dictionary<string, string> Keys = new Dictionary<string, string>
        {
            #region Common
            { "Check_updates","Check updates..." },
            { "Check_integrity","Check integrity..." },
            { "No_updates_required","No updates required." },
            { "Update_done","Update done." },
            { "Starting_download_update","Starting download update..." },
            { "Downloading_file","Downloading file - {0} - {1}..." },
            { "Check_file","Check file {0}..." },
            { "Сheck_complete","Сheck complete." },
            { "Reading","Reading..." },
            { "Read","Read..." },
            { "Extract","Extract {0}..." },
            { "Extract_completed","Extract completed." },
            { "Process_file","Process file {0}..." },
            { "Patch_done","Patch done." },
            #endregion
            #region Unity
             { "Loading","Loading..." },
            #endregion

            #region Errors
            { "No_permissions","No permissions. Please run this program as an administrator." },
             { "Unable_get_versions","Unable to get versions data!" },
             { "Unable_connect_server","Unable to connect to server, check internet connection.\nError Code: 101" },
             { "Unable_load_resource","Unable to load resource {0}.\nError Code: {1}" },
             { "Error_during_update","Error during program update!" },
             { "File_hashes_dont_match","File hashes do not match. Hash old:{0}. Entry hash old:{1}" },
             { "File_doesnt_match_hash_server","File hashes do not match. Hash old:{0}. Entry hash old:{1}" },
             #endregion
        };
    }
}
