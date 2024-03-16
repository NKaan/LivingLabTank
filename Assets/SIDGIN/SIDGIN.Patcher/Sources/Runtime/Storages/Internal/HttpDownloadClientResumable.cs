using SIDGIN.Patcher.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SIDGIN.Patcher.Storages
{
    public class DownloadProgressChanged
    {
        public readonly long BytesPerSecond;
        public readonly long BytesReceived;
        public DownloadProgressChanged(long bytes, long bytesPerSecond)
        {
            BytesPerSecond = bytesPerSecond;
            BytesReceived = bytes;
        }
    }
    internal class CookieWebClient
    {
        private class CookieContainer
        {
            Dictionary<string, string> _cookies;

            public string this[Uri url]
            {
                get
                {
                    string cookie;
                    if (_cookies.TryGetValue(url.Host, out cookie))
                        return cookie;

                    return null;
                }
                set
                {
                    _cookies[url.Host] = value;
                }
            }

            public CookieContainer()
            {
                _cookies = new Dictionary<string, string>();
            }
        }

        private CookieContainer cookies;

        public CookieWebClient()
        {
            cookies = new CookieContainer();
        }

        protected void SetСookies(HttpWebRequest request)
        {
            if (request is HttpWebRequest)
            {
                string cookie = cookies[request.Address];
                if (cookie != null)
                    ((HttpWebRequest)request).Headers.Set("cookie", cookie);
            }
        }

        protected void LoadСookies(HttpWebResponse response)
        {
            string[] cookies = response.Headers.GetValues("Set-Cookie");
            if (cookies != null && cookies.Length > 0)
            {
                string cookie = "";
                foreach (string c in cookies)
                    cookie += c;

                this.cookies[response.ResponseUri] = cookie;
            }
        }
    }
    internal class HttpDownloadClientResumable : CookieWebClient
    {

        public event Action<DownloadProgressChanged> ProgressChanged;

        public bool stop = true; // by default stop is true

        public virtual Task DownloadFileAsync(string downloadLink, string path,string targetHash, Func<HttpWebResponse, string> responsePathProcessor = null)
        {
            return Task.Run(async () =>
             {
                 try
                 {
                     stop = false;

                     var fileInfo = new FileInfo(path);
                     long existingLength = 0;
                     if (fileInfo.Exists)
                     {
                         existingLength = fileInfo.Length;
                         if (HashHelper.GetHashFromFile(path) == targetHash)
                             return;
                     }

                     var webRequest = HttpWebRequest.Create(downloadLink);
                     if (webRequest is FileWebRequest)
                     {
                         var webClient = new WebClient();
                         webClient.Headers.Add(HttpRequestHeader.UserAgent, "Other");
                         await webClient.DownloadFileTaskAsync(downloadLink, path);
                         return;
                     }
                     var request = (HttpWebRequest)webRequest;
                     request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.7) Gecko/2009021910 Firefox/3.0.7";
                     SetСookies(request);
                     request.Proxy = null;
                     request.AddRange(existingLength);

                     using (var response = (HttpWebResponse)request.GetResponse())
                     {
                         long fileSize = existingLength + response.ContentLength;
                         bool downloadResumable;
                         LoadСookies(response);
                         if (response.StatusCode == HttpStatusCode.PartialContent)
                         {
                             downloadResumable = true;
                         }
                         else
                         {
                             existingLength = 0;
                             downloadResumable = false;
                         }
                         if (responsePathProcessor != null)
                         {
                             var resultFilePath = responsePathProcessor.Invoke(response);
                             fileInfo = new FileInfo(resultFilePath);
                         }
                         using (var saveFileStream = fileInfo.Open(downloadResumable ? FileMode.Append : FileMode.Create, FileAccess.Write))
                         using (var stream = response.GetResponseStream())
                         {
                             byte[] downBuffer = new byte[4096];
                             int byteSize = 0;
                             long totalReceived = 0;
                             var sw = Stopwatch.StartNew();
                             while (!stop && (byteSize = stream.Read(downBuffer, 0, downBuffer.Length)) > 0)
                             {
                                 saveFileStream.Write(downBuffer, 0, byteSize);
                                 totalReceived += byteSize;
                                 var bytesPerSecond = totalReceived / sw.Elapsed.TotalSeconds;
                                 OnProgressChanged(new DownloadProgressChanged(totalReceived + existingLength, (long)bytesPerSecond));
                             }
                             sw.Stop();
                         }
                     }
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
                     throw new UnableConnectToServer(ex);
                 }
                 catch (Exception ex)
                 {
                     throw new UnableLoadResource(103, downloadLink, ex);
                 }

             });
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
