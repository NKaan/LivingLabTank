
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net;
using Amazon.S3.Model;

namespace SIDGIN.Patcher.Editors.Storages
{
    using Renci.SshNet;
    using Renci.SshNet.Async;
    using Renci.SshNet.Common;
    using Renci.SshNet.Sftp;
    using SIDGIN.Common.Editors;
    using SIDGIN.Patcher.Client;
    using SIDGIN.Patcher.Storages;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using static SIDGIN.Patcher.Editors.Storages.HttpSettingsData;


    using UnityEngine;
    using UnityEngine.Networking;
    using System.Security.Cryptography.X509Certificates;
    using System.Net.Security;

    public class AcceptAllCertificatesSignedWithASpecificKeyPublicKey : CertificateHandler
    {

        // Encoded RSAPublicKey
        private static string PUB_KEY = "3082010A0282010100AD3E1BBBF82BE073DC5756F57452DB15F1FE3AD0B62066EC91CCCFD7F3C4D6596C076A6152B16E47217C2B0C4143066176F8E3730E1D43D2DD9F7E01E5B88DC1AED991E8ED8FE42ACFB92E5F4E61F86807E31082F4006DB4413323793CC85C91C2E26E3F084F783311F722006AF4A57DA73214EE3014B45E5D048C33732A803BA027DD0EFA1A29F05A7D6DACD01C9968DF2F5C4A7D4145E3F3C118E55615F956AEC5CD2C3A753E08E5E136400D6D396E69AD682D59D49FDDA848B648FCB673CE6421602C2195CDC469D546B90E4F50C8862BE4FC5E3DCF3BB3B47855577173A912A43AA171498C7656DBEF441A456A5D68DDD8CC00533D2CBBCB20BC92B7BE150203010001";
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            X509Certificate2 certificate = new X509Certificate2(certificateData);
            string pk = certificate.GetPublicKeyString();
            Debug.Log("Key :" + pk);
            if (pk.Equals(PUB_KEY))
                return true;
            return false;
        }
        /* public static implicit operator CertificateHandler(AcceptAllCertificatesSignedWithASpecificKeyPublicKey v)
         {
             throw new NotImplementedException();
         }*/
    }

   

    partial class FtpApi : StorageApiEditor
    {
        // Обработчик событий для проверки сертификата
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            // Здесь вы можете реализовать вашу логику проверки сертификата.
            // Наиболее простым способом может быть просто возвращение true, чтобы принять любой сертификат.
            // В реальном приложении, возможно, вам захочется провести более детальную проверку.

            return true;
        }
        static string homeDirectory
        {
            get
            {
                var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
                string startString = "";
                string endString = "";
                if (string.IsNullOrEmpty(ftpSettings.ftpHomeDirectory))
                    return "/";
                if (ftpSettings.ftpHomeDirectory.Length > 0 && ftpSettings.ftpHomeDirectory[0] != '/')
                {
                    startString = "/";
                }
                if (ftpSettings.ftpHomeDirectory.Length > 1 && ftpSettings.ftpHomeDirectory[ftpSettings.ftpHomeDirectory.Length - 1] != '/')
                {
                    endString = "/";
                }
                return startString + ftpSettings.ftpHomeDirectory + endString;
            }
        }
        static string remoteDirectory
        {
            get
            {
                var settings = EditorSettingsLoader.Get<SettingsData>();
                string endString = "";
                if (settings.selectedDefinition.Length > 0 && settings.selectedDefinition[0] != '/')
                {
                    endString = "/";
                }
                return homeDirectory + settings.selectedDefinition + endString;
            }
        }
        static string httpBaseLink
        {
            get
            {
                var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
                var rootLink = ftpSettings.rootLink;
                if (ftpSettings.rootLink[ftpSettings.rootLink.Length - 1] != '/')
                {
                    rootLink += "/";
                }
                var settings = EditorSettingsLoader.Get<SettingsData>();
                return rootLink + settings.selectedDefinition + "/";
            }
        }
        static int blockSize = 1024;
        SftpClient sftpClient;
        void PrepareSftpClient()
        {
            try
            {
                if (sftpClient != null && sftpClient.IsConnected)
                    return;
            }
            catch /*(ObjectDisposedException e)*/
            {
                sftpClient = null;
            }
            var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
            string sftpDomain = ftpSettings.ftpDomain;
            PrivateKeyFile privateKey = null;
            if (System.IO.File.Exists(ftpSettings.privateKeyFilePath))
            {
                if (string.IsNullOrEmpty(ftpSettings.privateKeyPassphrase))
                {
                    privateKey = new PrivateKeyFile(ftpSettings.privateKeyFilePath);
                }
                else
                {
                    privateKey = new PrivateKeyFile(ftpSettings.privateKeyFilePath, ftpSettings.privateKeyPassphrase);
                }
            }
            var keyFiles = new[] { privateKey };

            var methods = new List<AuthenticationMethod>();

            if (privateKey != null)
            {
                methods.Add(new PrivateKeyAuthenticationMethod(ftpSettings.ftpUser, keyFiles));
            }
            else
            {
                methods.Add(new PasswordAuthenticationMethod(ftpSettings.ftpUser, ftpSettings.ftpPassword));
            }
            var connectionInfo = new ConnectionInfo(sftpDomain, Convert.ToInt32(ftpSettings.ftpPort), ftpSettings.ftpUser, methods.ToArray());

            sftpClient = new SftpClient(connectionInfo);
            try
            {
                sftpClient.Connect();
            }
            catch
            {
                sftpClient.Dispose();
                throw;
            }
        }


   


        FtpWebRequest PrepareFtpWebRequest(string additionalLink)
        {
            var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
            var ftpWebRequest = (FtpWebRequest)WebRequest.Create("ftp://" + ftpSettings.ftpDomain + "/" + additionalLink);
            ftpWebRequest.UseBinary = true;
            if (!string.IsNullOrEmpty(ftpSettings.ftpUser) && !string.IsNullOrEmpty(ftpSettings.ftpPassword))
                ftpWebRequest.Credentials = new NetworkCredential(ftpSettings.ftpUser, ftpSettings.ftpPassword);
            ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ftpWebRequest.EnableSsl = ftpSettings.enableSSL;
            ftpWebRequest.UseBinary = true;
            ftpWebRequest.KeepAlive = false;

            return ftpWebRequest;
        }
        static string GetParentUriString(string url)
        {
            var uri = new Uri(url);
            return uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);
        }

        void CreateDirectoryRecursively(string path)
        {
            string current = "";

            if (path[0] == '/')
            {
                path = path.Substring(1);
            }

            while (!string.IsNullOrEmpty(path))
            {
                int p = path.IndexOf('/');
                current += '/';
                if (p >= 0)
                {
                    current += path.Substring(0, p);
                    path = path.Substring(p + 1);
                }
                else
                {
                    current += path;
                    path = "";
                }

                CreateDirectory(current);
            }
        }
        void CreateDirectory(string folderPath)
        {
            var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
            if (!string.IsNullOrEmpty(folderPath))
            {
                if (ftpSettings.ftpProtocolType == FTPType.SFTP)
                {
                    PrepareSftpClient();
                    try
                    {
                        SftpFileAttributes attrs = sftpClient.GetAttributes(homeDirectory + folderPath);
                        if (!attrs.IsDirectory)
                        {
                            return;
                        }
                    }
                    catch (SftpPathNotFoundException)
                    {
                        sftpClient.CreateDirectory(homeDirectory + folderPath);
                    }
                }
                else
                {
                    try
                    {

                        var makeDirRequest = PrepareFtpWebRequest(homeDirectory + folderPath);
                        makeDirRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                        var response = (FtpWebResponse)makeDirRequest.GetResponse();
                        var ftpStream = response.GetResponseStream();
                        ftpStream.Close();
                        response.Close();
                    }
                    catch (WebException wex)
                    {
                        if (wex.Response != null)
                        {
                            var response = (FtpWebResponse)wex.Response;
                            if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                throw wex;
                            }
                        }
                        else
                        {
                            throw wex;
                        }
                    }
                }
            }
        }

        #region Upload

        long toSendBytes;
        public override Task<string> UploadFile(string filePath)
        {
            return Task.Run(() =>
            {
                var fileName = System.IO.Path.GetFileName(filePath);
                return UploadFile(filePath, fileName);
            });
        }
        public override Task<string> UploadFile(string filePath, string fileName)
        {
            return Task.Run(async () =>
            {
                var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
                var settings = EditorSettingsLoader.Get<SettingsData>();
                toSendBytes = new System.IO.FileInfo(filePath).Length;

                using (var stream = new System.IO.FileStream(filePath,
                                        System.IO.FileMode.Open))
                {
                    if (ftpSettings.ftpProtocolType == FTPType.SFTP)
                    {
                        try
                        {
                            PrepareSftpClient();
                            string uploadPath = remoteDirectory + fileName;
                            await sftpClient.UploadAsync(stream, uploadPath, uploadedBytes =>
                            {
                                ProgressChanged(new ProgressData
                                {
                                    targetBytes = toSendBytes,
                                    downloadedBytes = (long)uploadedBytes,
                                    progress = (float)((long)uploadedBytes / toSendBytes)
                                });
                            });
                            sftpClient.Disconnect();
                            sftpClient.Dispose();
                        }

                        catch (SftpPathNotFoundException)
                        {
                            stream.Close();
                            CreateDirectoryRecursively(settings.selectedDefinition);
                            await UploadFile(filePath, fileName);
                        }
                        catch
                        {
                            sftpClient.Disconnect();
                            sftpClient.Dispose();
                        }
                    }
                    else
                    {
                        try
                        {
                            string uploadPath = remoteDirectory + fileName;
                            var uploadRequest = PrepareFtpWebRequest(uploadPath);
                            uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;

                            var requestStream = await uploadRequest.GetRequestStreamAsync();

                            byte[] buffer = new byte[blockSize];
                            int bytesRead = 0;
                            long uploadedBytes = 0;
                            do
                            {

                                bytesRead = stream.Read(buffer, 0, blockSize);
                                if (bytesRead == 0)
                                {
                                    break;
                                }
                                requestStream.Write(buffer, 0, bytesRead);
                                uploadedBytes += bytesRead;
                                float progress = (float)(uploadedBytes / (float)toSendBytes);
                                ProgressChanged(new ProgressData { targetBytes = toSendBytes, downloadedBytes = uploadedBytes, progress = progress });
                            }
                            while (bytesRead != 0);
                            requestStream.Flush();
                            requestStream.Close();
                            stream.Close();
                            ProgressChanged(new ProgressData { targetBytes = toSendBytes, downloadedBytes = uploadedBytes, progress = 1 });
                        }

                        catch (WebException wex)
                        {
                            stream.Close();
                            if (wex.Response != null)
                            {
                                FtpWebResponse response = (FtpWebResponse)wex.Response;
                                if (response.StatusCode == FtpStatusCode.ActionNotTakenFilenameNotAllowed || response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                                {
                                    stream.Close();
                                    CreateDirectoryRecursively(settings.selectedDefinition);
                                    await UploadFile(filePath, fileName);
                                }
                            }
                            else
                            {
                                throw wex;
                            }
                        }
                    }
                }

                return httpBaseLink + fileName;
            });
        }
        bool isRecursiveRequest;
        public override Task<string> UploadData(string data, string fileName)
        {
            return Task.Run(async () =>
            {
                var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
                var settings = EditorSettingsLoader.Get<SettingsData>();

                if (ftpSettings.ftpProtocolType == FTPType.SFTP)
                {
                    try
                    {

                        PrepareSftpClient();
                        var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
                        string remotePath = remoteDirectory + fileName;
                        sftpClient.UploadFile(stream, remotePath);
                        sftpClient.Disconnect();
                        sftpClient.Dispose();
                        return httpBaseLink + fileName;
                    }

                    catch (SftpPathNotFoundException)
                    {
                        CreateDirectoryRecursively(settings.selectedDefinition);
                        return await UploadData(data, fileName);
                    }
                }
                else
                {
                    try
                    {
                        string remotePath = remoteDirectory + fileName;
                        var uploadRequest = PrepareFtpWebRequest(remotePath);
                        uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
                        var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        stream.Close();
                        var requestStream = await uploadRequest.GetRequestStreamAsync();
                        requestStream.Write(buffer, 0, buffer.Length);
                        requestStream.Flush();
                        requestStream.Close();
                        return httpBaseLink + fileName;
                    }
                    catch (WebException wex)
                    {
                        if (wex.Response != null)
                        {
                            FtpWebResponse response = (FtpWebResponse)wex.Response;
                            if (response.StatusCode == FtpStatusCode.ActionNotTakenFilenameNotAllowed || response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                if (!isRecursiveRequest)
                                {
                                    CreateDirectoryRecursively(settings.selectedDefinition);
                                    isRecursiveRequest = true;
                                    return await UploadData(data, fileName);
                                }
                                else
                                {
                                    isRecursiveRequest = false;
                                }
                            }

                        }
                        throw wex;

                    }
                }
            });
        }

        #endregion

        #region List

        public override Task<List<FileMeta>> GetListFileMeta()
        {
            return Task.Run(async () =>
            {
                var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
                var settings = EditorSettingsLoader.Get<SettingsData>();
                if (ftpSettings.ftpProtocolType == FTPType.SFTP)
                {
                    try
                    {
                        PrepareSftpClient();
                        var files = sftpClient.ListDirectory(remoteDirectory);
                        var resultList = new List<FileMeta>();
                        foreach (var file in files)
                        {
                            if (file.Name != ".." && file.Name != "." && file.Name != "")
                            {
                                try
                                {
                                    long? size = file.Attributes.Size;
                                    resultList.Add(new FileMeta(file.Name, httpBaseLink + file.Name, file.Name,
                                        size.HasValue ? size.Value : -1));
                                }
                                catch (Exception e)
                                {
                                    throw e;
                                }
                            }
                        }

                        return resultList;
                    }
                    catch (SftpPathNotFoundException)
                    {
                        CreateDirectoryRecursively(settings.selectedDefinition);
                        return await GetListFileMeta();
                    }
                    finally
                    {
                        sftpClient.Disconnect();
                        sftpClient.Dispose();
                    }
                }
                else
                {
                    var listRequest = PrepareFtpWebRequest(remoteDirectory);
                    listRequest.Method = WebRequestMethods.Ftp.ListDirectory;

                    FtpWebResponse listResponse = (FtpWebResponse) listRequest.GetResponse();
                    System.IO.Stream responseStream = listResponse.GetResponseStream();
                    System.IO.StreamReader reader = new System.IO.StreamReader(responseStream);
                    List<string> names = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        names.Add(reader.ReadLine());
                    }

                    reader.Close();
                    var resultList = new List<FileMeta>();
                    listResponse.Close();
                    names.Remove("..");
                    names.Remove(".");
                    foreach (var name in names)
                    {
                        resultList.Add(new FileMeta(name, name, name, -1));
                    }

                    return resultList;
                }
            });
        }

        #endregion

        #region Delete

        public override Task DeleteFile(string id)
        {
            return Task.Run(() =>
            {
                var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
                var settings = EditorSettingsLoader.Get<SettingsData>();
                var fileName = Path.GetFileName(id);
                if (ftpSettings.ftpProtocolType == FTPType.SFTP)
                {
                    PrepareSftpClient();
                    try
                    {
                        string additionalUrl = remoteDirectory + fileName;
                        sftpClient.DeleteFile(additionalUrl);
                    }
                    finally
                    {
                        sftpClient.Disconnect();
                        sftpClient.Dispose();
                    }
                }
                else
                {
                    string additionalUrl = remoteDirectory + fileName;
                    var deleteRequest = PrepareFtpWebRequest(additionalUrl);
                    deleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    var response = (FtpWebResponse) deleteRequest.GetResponse();
                    response.Close();
                }
            });
        }

        #endregion

        #region Download

        public override async Task<string> GetVersionsLink()
        {
            return httpBaseLink + Consts.APP_DATA_FILE_NAME;
        }

        #region Download

        public override Task<string> DownloadDataByName(string name)
        {
            return DownloadData(name);
        }

        public override async Task DownloadFile(string id, string filePath)
        {
            var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
            if (ftpSettings.ftpProtocolType == FTPType.SFTP)
            {
                try
                {
                    using (Stream fileStream = File.Open(filePath, FileMode.OpenOrCreate))
                    {
                        PrepareSftpClient();
                        await sftpClient.DownloadAsync(remoteDirectory + id, fileStream);
                    }
                }
                catch (SftpPathNotFoundException ex)
                {
                    throw new UnableLoadResource(102, remoteDirectory + id, ex);
                }
                catch (Exception ex)
                {
                    throw new UnableLoadResource(103, remoteDirectory + id, ex);
                }
            }
            else
            {
                try
                {
                    FtpWebRequest request = PrepareFtpWebRequest(remoteDirectory + id);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;

                    Stream requestStream = (await request.GetResponseAsync()).GetResponseStream();
                    using (Stream fileStream = File.Open(filePath, FileMode.OpenOrCreate))
                    {
                        requestStream.CopyTo(fileStream);
                    }

                    requestStream.Flush();
                    requestStream.Close();

                }
                catch (WebException ex)
                {
                    if (ex.InnerException != null && ex.InnerException is UnauthorizedAccessException)
                    {
                        throw new UnableLoadResource(101, remoteDirectory + id, ex);
                    }

                    if (ex.Response is HttpWebResponse)
                    {
                        var response = (HttpWebResponse) ex.Response;
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new UnableLoadResource(102, remoteDirectory + id, ex);
                        }
                        else
                        {
                            throw new UnableLoadResource((int) response.StatusCode, remoteDirectory + id, ex);
                        }
                    }

                    if (ex.Response is FtpWebResponse ftpWebResponse)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFilenameNotAllowed ||
                            ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            throw new UnableLoadResource((int) ftpWebResponse.StatusCode, remoteDirectory + id, ex);
                        }
                    }

                    if (ex.Message.Contains("File Not Found"))
                    {
                        throw new UnableLoadResource(102, remoteDirectory + id, ex);
                    }

                    if (ex.Message.Contains("Directory Not Found"))
                    {
                        throw new UnableLoadResource(102, remoteDirectory + id, ex);
                    }

                    
                    if (ex.Message.Contains("No such file"))
                    {
                        throw new UnableLoadResource(102, remoteDirectory + id, ex);
                    }
                    
                    throw new UnableConnectToServer(ex);
                }
                catch (Exception ex)
                {
                    throw new UnableLoadResource(103, remoteDirectory + id, ex);
                }
            }
        }
        public override async Task<string> DownloadData(string id)
        {
            var ftpSettings = EditorSettingsLoader.Get<HttpSettingsData>();
            if (ftpSettings.ftpProtocolType == FTPType.SFTP)
            {
                try
                {

                    using (var stream = new System.IO.MemoryStream())
                    {
                        PrepareSftpClient();
                        await sftpClient.DownloadAsync(remoteDirectory + id, stream);
                        stream.Position = 0;
                        StreamReader reader = new StreamReader(stream);
                        string text = reader.ReadToEnd();
                        reader.Close();
                        return text;
                    }
                }
                catch (SftpPathNotFoundException ex)
                {
                    throw new UnableLoadResource(102, remoteDirectory + id, ex);
                }
                catch (Exception ex)
                {
                    throw new UnableLoadResource(103, remoteDirectory + id, ex);
                }
            }
            else
            {

                try
                {
                    FtpWebRequest request = PrepareFtpWebRequest(remoteDirectory + id);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    string resultData = "";
                    Stream requestStream = (await request.GetResponseAsync()).GetResponseStream();
                    using (StreamReader reader = new StreamReader(requestStream, Encoding.UTF8))
                    {
                        resultData = reader.ReadToEnd();
                    }

                    requestStream.Flush();
                    requestStream.Close();
                    return resultData;
                }
                catch (WebException ex)
                {
                    if (ex.InnerException != null && ex.InnerException is UnauthorizedAccessException)
                    {
                        throw new UnableLoadResource(101, remoteDirectory + id, ex);
                    }

                    if (ex.Response is HttpWebResponse)
                    {
                        var response = (HttpWebResponse) ex.Response;
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            throw new UnableLoadResource(102, remoteDirectory + id, ex);
                        }
                        else
                        {
                            throw new UnableLoadResource((int) response.StatusCode, remoteDirectory + id, ex);
                        }
                    }

                    if (ex.Response is FtpWebResponse ftpWebResponse)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFilenameNotAllowed ||
                            ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            throw new UnableLoadResource((int) ftpWebResponse.StatusCode, remoteDirectory + id, ex);
                        }
                    }

                    if (ex.Message.Contains("File Not Found"))
                    {
                        throw new UnableLoadResource(102, remoteDirectory + id, ex);
                    }

                    if (ex.Message.Contains("Directory Not Found"))
                    {
                        throw new UnableLoadResource(102, remoteDirectory + id, ex);
                    }
  
                    if (ex.Message.Contains("No such file"))
                    {
                        throw new UnableLoadResource(102, remoteDirectory + id, ex);
                    }
                    
                    throw new UnableConnectToServer(ex);
                }
                catch (Exception ex)
                {
                    throw new UnableLoadResource(103, remoteDirectory + id, ex);
                }
            }

        }

        #endregion
        public override async Task<string> GetFileIdByName(string name)
        {
            var link = httpBaseLink + name;
            return link;
        }
        #endregion

    }
    public static class HTTPApiExtensions
    {
        public static FileMeta ToFileMeta(string filename, long? size)
        {
            var result = new FileMeta(filename, filename, filename, size.HasValue ? size.Value : -1);
            return result;
        }
    }
}
