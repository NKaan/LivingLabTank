#if SGPATCHER_GOOGLE
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIDGIN.Patcher.Editors.Storages
{
    using Common;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Drive.v3;
    using Google.Apis.Services;
    using Google.Apis.Drive.v3.Data;
    using Google.Apis.Util.Store;
    using Google.Apis.Download;
    using Google;
    using System.Net.Http;
    using System.Net;
    using SIDGIN.Common.Editors;
    using SIDGIN.Patcher.Storages;
    using SIDGIN.Patcher.Client;

    class GoogleDriveApiEditor : StorageApiEditor
    {
        const string api_data = "{\"installed\":{\"client_id\":\"747518414568-lkslgs73od9bir44akj1mt1c6m1e0epk.apps.googleusercontent.com\",\"project_id\":\"sg-patcher\",\"auth_uri\":\"https://accounts.google.com/o/oauth2/auth\",\"token_uri\":\"https://oauth2.googleapis.com/token\",\"auth_provider_x509_cert_url\":\"https://www.googleapis.com/oauth2/v1/certs\",\"client_secret\":\"b-9nvXEroxEUJzYsNDQc3EOo\",\"redirect_uris\":[\"urn:ietf:wg:oauth:2.0:oob\",\"http://localhost\"]}}";
        DriveService driveService;
        static System.IO.Stream GetStreamFromString(string s)
        {
            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        DriveService LoadService()
        {
            PrepareCertificate();
            if (driveService != null)
                return driveService;
            string[] scopes = new string[] { DriveService.Scope.Drive };
            UserCredential credential;
            var credentialsStream = GetStreamFromString(api_data);

            string credPath = "sgpatcher_token";
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(credentialsStream).Secrets,
                scopes,
                "user",
                CancelationHelper.CancelToken,
                new FileDataStore(credPath, true)).Result;

            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "SG Patcher"
            });
            driveService = service;
            return driveService;
        }
        async Task<FileMeta> CreateFolder(string name, string parentId="")
        {
            List<string> parents = null;
            if (!string.IsNullOrEmpty(parentId))
            {
                parents = new List<string> { parentId };
            }
            var folderMetadata = new File()
            {
                Parents = parents,
                Name = name,
                MimeType = "application/vnd.google-apps.folder"
            };
            var request = driveService.Files.Create(folderMetadata);
            request.Fields = "id";
            var folder = await request.ExecuteAsync(CancelationHelper.CancelToken);
            return folder.ToFileMeta();
        }
        async Task<FileMeta> GetCurrentFolder()
        {
            var googleSettings = EditorSettingsLoader.Get<GoogleSettingsData>();
            var settings = EditorSettingsLoader.Get<SettingsData>();
            var rootFolder = await GetMetaData(googleSettings.sharedFolder);
            if (rootFolder == null)
            {
                rootFolder = await CreateFolder(googleSettings.sharedFolder);
            }
            var folderMeta = await GetMetaData(settings.selectedDefinition, rootFolder.id);
            if (folderMeta == null)
            {
                folderMeta = await CreateFolder(settings.selectedDefinition, rootFolder.id);
            }
            return folderMeta;
        }
        async Task<FileMeta> GetMetaDataById(string id)
        {

            var driveService = LoadService();
            var metaReq = driveService.Files.List();
            metaReq.Spaces = "drive";
            metaReq.Fields = "files(id, name, size)";
            metaReq.Q = $"id = '{id}'";
            try
            {
                var result = await metaReq.ExecuteAsync(CancelationHelper.CancelToken);
                var data = result.Files.FirstOrDefault();
                if (data == null)
                    return null;
                return data.ToFileMeta();
            }
            catch
            {
                return null;
            }
        }
        async Task<FileMeta> GetMetaData(string name)
        {
            
            var driveService = LoadService();
            var metaReq = driveService.Files.List();
            metaReq.Spaces = "drive";
            metaReq.Fields = "files(id, name, size)";
            metaReq.Q = $"name = '{name}'";
            try
            {
                var result = await metaReq.ExecuteAsync(CancelationHelper.CancelToken);
                var data = result.Files.FirstOrDefault();
                if (data == null)
                    return null;
                return data.ToFileMeta();
            }
            catch
            {
                return null;
            }
        }
        async Task<FileMeta> GetMetaData(string name, string folder)
        {
            var driveService = LoadService();
            var metaReq = driveService.Files.List();
            metaReq.Spaces = "drive";
            metaReq.Fields = "files(id, name, size)";
            metaReq.Q = string.Format("name='{0}' and '{1}' in parents", name, folder);
            try
            {
                var result = await metaReq.ExecuteAsync(CancelationHelper.CancelToken);
                var data = result.Files.FirstOrDefault();
                if (data == null)
                    return null;
                return data.ToFileMeta();
            }
            catch (GoogleApiException ex)
            {
                if (ex.Error != null)
                {
                    if (ex.Error.Code == 404)
                    {
                        return null;
                    }
                }
                throw ex;
            }
        }
        public override async Task<string> GetVersionsLink()
        {
            var folderMeta = await GetCurrentFolder();
            var meta = await GetMetaData(Consts.APP_DATA_FILE_NAME, folderMeta.id);
            if (meta == null)
            {
                return "";
            }
            return meta.id;
        }
        #region Upload
        long toSendBytes;
        public override async Task<string> UploadFile(string filePath, string fileName)
        {
            var driveService = LoadService();
            var folderMeta = await GetCurrentFolder();
            var existMetaData = await GetMetaData(fileName, folderMeta.id);
            toSendBytes = new System.IO.FileInfo(filePath).Length;
            var fileMetadata = new File()
            {
                Name = fileName,
            };
            using (var stream = new System.IO.FileStream(filePath,
                                    System.IO.FileMode.Open))
            {
                if (existMetaData != null)
                {
                    var uploadRequest = driveService.Files.Update(fileMetadata, existMetaData.id, stream, "application/patch");
                    uploadRequest.Fields = "id";
                    uploadRequest.ProgressChanged += Request_ProgressChanged;
                    uploadRequest.ChunkSize = FilesResource.CreateMediaUpload.MinimumChunkSize;
                    var result = await uploadRequest.UploadAsync(CancelationHelper.CancelToken);
                    if (result.Status == Google.Apis.Upload.UploadStatus.Failed)
                    {
                        throw result.Exception;
                    }
                    return uploadRequest.ResponseBody.Id;
                }
                else
                {

                    fileMetadata.Parents = new List<string>
                    {
                           folderMeta.id
                    };
                    var uploadRequest = driveService.Files.Create(
                    fileMetadata, stream, "application/patch");
                    uploadRequest.Fields = "id";
                    uploadRequest.ProgressChanged += Request_ProgressChanged;
                    uploadRequest.ChunkSize = FilesResource.CreateMediaUpload.MinimumChunkSize;
                    var result = await uploadRequest.UploadAsync(CancelationHelper.CancelToken);
                    if (result.Status == Google.Apis.Upload.UploadStatus.Failed)
                    {
                        throw result.Exception;
                    }
                    await ShareFile(driveService, uploadRequest.ResponseBody.Id);
                    return uploadRequest.ResponseBody.Id;
                }

            }
        }
        public override async Task<string> UploadFile(string filePath)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            return await UploadFile(filePath, fileName);
        }
        static async Task ShareFile(DriveService driveService, string id)
        {
            var permission = new Permission();
            permission.Role = "reader";
            permission.Type = "anyone";

            var request = driveService.Permissions.Create(permission, id);
            await request.ExecuteAsync();
        }
        private void Request_ProgressChanged(Google.Apis.Upload.IUploadProgress obj)
        {
            ProgressChanged(new ProgressData { progress = (float)((double)obj.BytesSent / (double)toSendBytes), downloadedBytes = obj.BytesSent, targetBytes = toSendBytes });
        }
        public override async Task<string> UploadData(string data, string fileName)
        {
            var driveService = LoadService();
            var folderMeta = await GetCurrentFolder();
            var existMetaData = await GetMetaData(fileName,folderMeta.id);
            var fileMetadata = new File()
            {
                Name = fileName,
            };
            using (var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(data)))
            {
                if (existMetaData != null)
                {
                   
                    var uploadRequest = driveService.Files.Update(
                        fileMetadata, existMetaData.id, stream, "application/patch");

                    uploadRequest.Fields = "id";
                    uploadRequest.ChunkSize = FilesResource.CreateMediaUpload.MinimumChunkSize;
                    var result = await uploadRequest.UploadAsync(CancelationHelper.CancelToken);
                    if (result.Status == Google.Apis.Upload.UploadStatus.Failed)
                    {
                        throw result.Exception;
                    }
                    return uploadRequest.ResponseBody.Id;
                }
                else
                {
                    fileMetadata.Parents = new List<string>
                    {
                           folderMeta.id
                    };
                    var uploadRequest = driveService.Files.Create(
                       fileMetadata, stream, "application/patch");

                    uploadRequest.Fields = "id";
                    uploadRequest.ChunkSize = FilesResource.CreateMediaUpload.MinimumChunkSize;
                    var result = await uploadRequest.UploadAsync();
                    if (result.Status == Google.Apis.Upload.UploadStatus.Failed)
                    {
                        throw result.Exception;
                    }
                    await ShareFile(driveService, uploadRequest.ResponseBody.Id);
                    return uploadRequest.ResponseBody.Id;
                }
            }
           
        }
        #endregion

        public override async Task<string> GetFileIdByName(string name)
        {
            var settings = EditorSettingsLoader.Get<SettingsData>();
            var fileMeta = await GetMetaData(name, settings.selectedDefinition);
            if (fileMeta != null)
                return fileMeta.id;
            return "";
        }

        #region List
        public override async Task<List<FileMeta>> GetListFileMeta()
        {
            var driveService = LoadService();

            var folderMeta = await GetCurrentFolder();

            var request = driveService.Files.List();
            request.Spaces = "drive";
            request.Fields = "nextPageToken, files(id, name, size)";
            request.PageToken = "";
            request.Q = string.Format("'{0}' in parents", folderMeta.id);
            var resultList = new List<FileMeta>();
            var result = await request.ExecuteAsync(CancelationHelper.CancelToken);
            foreach (var file in result.Files)
            {
                resultList.Add(file.ToFileMeta());
            }
            return resultList;
        }
        #endregion

        #region Delete
        public override async Task DeleteFile(string id)
        {
            var driveService = LoadService();
            var request = driveService.Files.Delete(id);
            await request.ExecuteAsync(CancelationHelper.CancelToken);
        }
        #endregion
        #region Download
        public override async Task DownloadFile(string id, string filePath)
        {
            try
            {
                var driveService = LoadService();

                var metaData = await GetMetaDataById(id);
                var request = driveService.Files.Get(id);
                request.MediaDownloader.ChunkSize = 1024;
                request.MediaDownloader.ProgressChanged +=
            (IDownloadProgress progress) =>
            {
                if (progress.Status == DownloadStatus.Downloading)
                {
                    float p = 0;
                    if (metaData != null && metaData.size != 0)
                    {
                        p = (float) ((double) progress.BytesDownloaded / (double) metaData.size);
                    }
                    ProgressChanged(new ProgressData
                    {
                        progress = p,
                        downloadedBytes = progress.BytesDownloaded,
                        targetBytes = metaData != null ? metaData.size : 0
                    });
                }
            };
                using (var stream = new System.IO.FileStream(filePath,
                                        System.IO.FileMode.OpenOrCreate))
                {
                    var result = await request.DownloadAsync(stream, CancelationHelper.CancelToken);
                    if (result.Status == Google.Apis.Download.DownloadStatus.Failed)
                    {
                        throw result.Exception;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException webException)
                {
                    if (webException.Status == WebExceptionStatus.ConnectFailure)
                    {
                        throw new UnableConnectToServer(ex);
                    }

                }
                throw ex;
            }
            catch (GoogleApiException ex)
            {
                if (ex.Error != null)
                {
                    if (ex.Error.Code == 404)
                    {
                        throw new UnableLoadResource(102, id);
                    }
                }
                throw ex;
            }


        }
        public override async Task<string> DownloadData(string id)
        {
            try
            {
                var driveService = LoadService();
                string resultString;
                var request = driveService.Files.Get(id);
                using (var stream = new System.IO.MemoryStream())
                {
                    var result = await request.DownloadAsync(stream, CancelationHelper.CancelToken);
                    if (result.Status == Google.Apis.Download.DownloadStatus.Failed)
                    {
                        throw result.Exception;
                    }
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    using (var streamReader = new System.IO.StreamReader(stream))
                    {
                        resultString = await streamReader.ReadToEndAsync();
                    }
                }
                return resultString;
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException webException)
                {
                    if (webException.Status == WebExceptionStatus.ConnectFailure)
                    {
                        throw new UnableConnectToServer(ex);
                    }

                }
                throw ex;
            }
            catch (GoogleApiException ex)
            {
                if (ex.Error != null)
                {
                    if (ex.Error.Code == 404)
                    {
                        throw new UnableLoadResource(102, id);
                    }
                }
                throw ex;
            }
        }
        public override async Task<string> DownloadDataByName(string name)
        {
            try
            {
                var folderMeta = await GetCurrentFolder();
                var meta = await GetMetaData(name, folderMeta.id);
                if (meta == null)
                {
                    throw new UnableLoadResource(102, name);
                }
                return await DownloadData(meta.id);
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }

    public static class GoogleDriveApiExtensions
    {
        public static FileMeta ToFileMeta(this File source)
        {
            var result = new FileMeta(source.Id,source.Id, source.Name, source.Size.HasValue ? source.Size.Value : -1);
            return result;
        }
    }
}
#endif