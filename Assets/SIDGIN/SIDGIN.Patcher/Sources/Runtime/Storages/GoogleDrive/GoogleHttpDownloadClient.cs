using SIDGIN.Patcher.Common;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SIDGIN.Patcher.Storages
{
    internal class GoogleHttpDownloadClient : HttpDownloadClientResumable
    {
        string tempFolder;
        public GoogleHttpDownloadClient()
        {
            tempFolder = TempHelper.GetTempFolder();
        }
        const string GOOGLE_DRIVE_LINK_BASE = "https://drive.google.com/uc?id={0}&export=download";

        public override async Task DownloadFileAsync(string id, string path,string targetHash, Func<HttpWebResponse, string> responsePathProcessor = null)
        {
            var url = GetGoogleDriveDownloadLinkFromId(id);

            string confirmFilePath = path + "_confirm";
            await base.DownloadFileAsync(url, path, targetHash, response =>
             {
                 if(response.StatusCode == HttpStatusCode.PartialContent)
                 {
                     return path;
                 }
                 else
                 {
                     return confirmFilePath;
                 }
             });


            FileInfo confirmFile = new FileInfo(confirmFilePath);

            // Confirmation page is around 50KB, shouldn't be larger than 60KB
            if (confirmFile.Exists && ConfirmGoogleLink(confirmFile, out url))
            {
                try
                {
                    File.Delete(confirmFilePath);
                }
                catch { }
            }
            else
            {
                return;
            }

            await base.DownloadFileAsync(url, path,targetHash, responsePathProcessor);
        }
       
        public override async Task<string> DownloadDataAsync(string id)
        {
            var tmpFile = Path.Combine(tempFolder, Guid.NewGuid().ToString() + ".tmp");
            await DownloadFileAsync(id, tmpFile, "");

            var data = File.ReadAllText(tmpFile);
            try
            {
                File.Delete(tmpFile);
            }
            catch { }
            return data;
        }

        static bool ConfirmGoogleLink(FileInfo file, out string url)
        {
            url = "";
            // Confirmation page is around 50KB, shouldn't be larger than 60KB
            if (file.Length > 60000) 
                return false;

            // Downloaded file might be the confirmation page, check it
            string content;
            using (var reader = file.OpenText())
            {
                // Confirmation page starts with <!DOCTYPE html>, which can be preceeded by a newline
                char[] header = new char[20];
                int readCount = reader.ReadBlock(header, 0, 20);
                if (readCount < 20 || !(new string(header).Contains("<!DOCTYPE html>")))
                    return false;

                content = reader.ReadToEnd();
            }

            int linkIndex = content.LastIndexOf("href=\"/uc?");
            if (linkIndex < 0)
                return false;

            linkIndex += 6;
            int linkEnd = content.IndexOf('"', linkIndex);
            if (linkEnd < 0)
                return false;
            url = "https://drive.google.com" + content.Substring(linkIndex, linkEnd - linkIndex).Replace("&amp;", "&");
            return true;
        }
        static string GetGoogleDriveDownloadLinkFromId(string id)
        {
            return string.Format(GOOGLE_DRIVE_LINK_BASE, id);
        }
    }
  
}
