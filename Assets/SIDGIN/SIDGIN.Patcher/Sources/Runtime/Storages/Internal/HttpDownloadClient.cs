using SIDGIN.Patcher.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace SIDGIN.Patcher.Storages
{
    internal class HttpDownloadClient
    {

        public event Action<DownloadProgressChanged> ProgressChanged;

        public bool stop = true; // by default stop is true
        Stopwatch timer;
        public virtual Task DownloadFileAsync(string downloadLink, string path,string targetHash, Func<HttpWebResponse, string> responsePathProcessor = null)
        {
            return Task.Run(async () =>
             {
                 try
                 {
                     var webClient = new WebClient();
                     webClient.Headers.Add(HttpRequestHeader.UserAgent, "Other");
                     webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                     timer = Stopwatch.StartNew();
                     await webClient.DownloadFileTaskAsync(new Uri(downloadLink), path);
                     timer.Stop();
                 }
                 catch (WebException ex)
                 {
                     if (ex.InnerException != null && ex.InnerException is UnauthorizedAccessException)
                     {
                         throw new UnableLoadResource(101, downloadLink, ex);
                     }
                     if (ex.Response is HttpWebResponse)
                     {
                         var response = (HttpWebResponse)ex.Response;
                         if (response.StatusCode == HttpStatusCode.NotFound)
                         {
                             throw new UnableLoadResource(102, downloadLink, ex);
                         }
                         else
                         {
                             throw new UnableLoadResource((int)response.StatusCode, downloadLink, ex);
                         }
                     }
                     if (ex.Message.Contains("File Not Found"))
                     {
                         throw new UnableLoadResource(102, downloadLink, ex);
                     }
                     if (ex.Message.Contains("Directory Not Found"))
                     {
                         throw new UnableLoadResource(102, downloadLink, ex);
                     }
                     throw new UnableConnectToServer(ex);
                 }
                 catch (Exception ex)
                 {
                     throw new UnableLoadResource(103, downloadLink, ex);
                 }

             });
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (timer != null)
            {
                OnProgressChanged(new DownloadProgressChanged(e.BytesReceived, (long)((float)e.BytesReceived / timer.Elapsed.TotalSeconds)));
            }
            else
            {
                OnProgressChanged(new DownloadProgressChanged(e.BytesReceived, 1));
            }
        }

        public virtual async Task<string> DownloadDataAsync(string downloadLink)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Other");
                return await webClient.DownloadStringTaskAsync(downloadLink);
            }
            catch (WebException ex)
            {
                if (ex.InnerException != null && ex.InnerException is UnauthorizedAccessException)
                {
                    throw new UnableLoadResource(101, downloadLink, ex);
                }
                if (ex.Response is HttpWebResponse)
                {
                    var response = (HttpWebResponse)ex.Response;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new UnableLoadResource(102, downloadLink, ex);
                    }
                    else
                    {
                        throw new UnableLoadResource((int)response.StatusCode, downloadLink, ex);
                    }
                }
                if (ex.Message.Contains("File Not Found"))
                {
                    throw new UnableLoadResource(102, downloadLink, ex);
                }
                if (ex.Message.Contains("Directory Not Found"))
                {
                    throw new UnableLoadResource(102, downloadLink, ex);
                }
                throw new UnableConnectToServer(ex);
            }
            catch (Exception ex)
            {
                throw new UnableLoadResource(103, downloadLink, ex);
            }
        }
        
       
        void OnProgressChanged(DownloadProgressChanged e)
        {
            var handler = ProgressChanged;
            if (handler != null)
            {
                handler(e);
            }
        }

    }


}
