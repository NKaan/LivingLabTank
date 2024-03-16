
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Diagnostics;
using SIDGIN.Common.Editors;
using SIDGIN.Patcher.Storages;
using SIDGIN.Patcher.Client;

namespace SIDGIN.Patcher.Editors.Storages
{ 
    public class AmazonApiEditor : StorageApiEditor
    {

        static System.IO.Stream GetStreamFromString(string s)
        {
            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string ConvertEndpointToString(AmazonEndpoint endpoint)
        {
            switch (endpoint)
            {
                case AmazonEndpoint.USEast1:
                    return "us-east-1";
                case AmazonEndpoint.AFSouth1:
                    return "af-east-1";
                case AmazonEndpoint.MESouth1:
                    return "me-east-1";
                case AmazonEndpoint.CACentral1:
                    return "ca-central-1";
                case AmazonEndpoint.CNNorthWest1:
                    return "cn-northwest-1";
                case AmazonEndpoint.CNNorth1:
                    return "cn-north-1";
                case AmazonEndpoint.APSoutheast2:
                    return "ap-southeast-2";
                case AmazonEndpoint.APSoutheast1:
                    return "ap-southeast-1";
                case AmazonEndpoint.APSouth1:
                    return "ap-south-1";
                case AmazonEndpoint.APNortheast3:
                    return "ap-northeast-3";
                case AmazonEndpoint.SAEast1:
                    return "sa-east-1";
                case AmazonEndpoint.APNortheast1:
                    return "ap-northeast-1";
                case AmazonEndpoint.APNortheast2:
                    return "ap-northeast-2";
                case AmazonEndpoint.USWest1:
                    return "us-west-1";
                case AmazonEndpoint.USWest2:
                    return "us-west-2";
                case AmazonEndpoint.EUNorth1:
                    return "eu-north-1";
                case AmazonEndpoint.EUWest1:
                    return "eu-west-1";
                case AmazonEndpoint.USEast2:
                    return "us-east-2";
                case AmazonEndpoint.EUWest3:
                    return "eu-west-3";
                case AmazonEndpoint.EUCentral1:
                    return "eu-central-1";
                case AmazonEndpoint.EUSouth1:
                    return "eu-south-1";
                case AmazonEndpoint.APEast1:
                    return "ap-east-1";
                case AmazonEndpoint.EUWest2:
                    return "eu-west-2";
                default:
                    return "us-east-1";
            }
        }
        long toSendBytes;
        public override async Task<string> UploadFile(string filePath, string fileName)
        {
            toSendBytes = new System.IO.FileInfo(filePath).Length;
            var awsSettings = EditorSettingsLoader.Get<AmazonSettingsData>();
            var settings = EditorSettingsLoader.Get<SettingsData>();
            using (var client = new AmazonS3Client(awsSettings.accessKeyId, awsSettings.secretAccessKey, RegionEndpoint.GetBySystemName(ConvertEndpointToString(awsSettings.endpoint))))
            {
                try
                {
                    var fileTransferUtility = new TransferUtility(client);
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        FilePath = filePath,
                        BucketName = awsSettings.bucketName,
                        CannedACL = S3CannedACL.PublicRead,
                        Key = awsSettings.rootFolder + "/" + settings.selectedDefinition + "/" + fileName,
                    };
                    uploadRequest.UploadProgressEvent += UploadRequest_UploadProgressEvent;
                    await fileTransferUtility.UploadAsync(uploadRequest);

                    GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                    {
                        BucketName = awsSettings.bucketName,
                        Key = awsSettings.rootFolder + "/" + settings.selectedDefinition + "/" + fileName,
                        Expires = DateTime.Now.AddMinutes(1)
                    };
                    
                    var presignedLink = client.GetPreSignedURL(request);
                    var link = presignedLink.Substring(0, presignedLink.IndexOf("?X-Amz"));
                    link = link.Replace(GetBadAmazonDomainLink(), GetAmazonDomainLink());
                    return link;
                }
                catch (AmazonS3Exception e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private void UploadRequest_UploadProgressEvent(object sender, UploadProgressArgs e)
        {
            ProgressChanged(new ProgressData { progress = (float)((double)e.TransferredBytes / (double)toSendBytes), downloadedBytes = e.TransferredBytes, targetBytes = toSendBytes });
        }

        public override async Task<string> UploadFile(string filePath)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            return await UploadFile(filePath, fileName);
        }
        public override async Task<string> UploadData(string data, string fileName)
        {
            var stream = GetStreamFromString(data);
            var awsSettings = EditorSettingsLoader.Get<AmazonSettingsData>();
            var settings = EditorSettingsLoader.Get<SettingsData>();

            using (var client = new AmazonS3Client(awsSettings.accessKeyId, awsSettings.secretAccessKey, RegionEndpoint.GetBySystemName(ConvertEndpointToString(awsSettings.endpoint))))
            {

                List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

                InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
                {
                    BucketName = awsSettings.bucketName,
                    Key = awsSettings.rootFolder + "/" + settings.selectedDefinition + "/" + fileName,
                    CannedACL = S3CannedACL.PublicRead,
                };

                InitiateMultipartUploadResponse initResponse =
                    await client.InitiateMultipartUploadAsync(initiateRequest);

                long contentLength = data.Length;
                long partSize = 5 * (long)Math.Pow(2, 20);

                try
                {
                    long filePosition = 0;
                    for (int i = 1; filePosition < contentLength; i++)
                    {
                        UploadPartRequest uploadRequest = new UploadPartRequest
                        {
                            BucketName = awsSettings.bucketName,
                            Key = awsSettings.rootFolder + "/" + settings.selectedDefinition + "/" + fileName,
                            UploadId = initResponse.UploadId,
                            PartNumber = i,
                            PartSize = partSize,
                            FilePosition = filePosition,
                            InputStream = stream,
                        };

                        // Upload a part and add the response to our list.
                        uploadResponses.Add(await client.UploadPartAsync(uploadRequest));

                        filePosition += partSize;
                    }

                    // Setup to complete the upload.
                    CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
                    {
                        BucketName = awsSettings.bucketName,
                        Key = awsSettings.rootFolder + "/" + settings.selectedDefinition + "/" + fileName,
                        UploadId = initResponse.UploadId
                    };
                    completeRequest.AddPartETags(uploadResponses);

                    // Complete the upload.
                    CompleteMultipartUploadResponse completeUploadResponse =
                        await client.CompleteMultipartUploadAsync(completeRequest);

                    GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                    {
                        BucketName = awsSettings.bucketName,
                        Key = awsSettings.rootFolder + "/" + settings.selectedDefinition + "/" + fileName,
                        Expires = DateTime.Now.AddMinutes(1)
                    };
                    var presignedLink = client.GetPreSignedURL(request);
                    var link = presignedLink.Substring(0, presignedLink.IndexOf("?X-Amz"));
                    link = link.Replace(GetBadAmazonDomainLink(), GetAmazonDomainLink());
                    return link;
                }
                catch (Exception exception)
                {
                    AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                    {
                        BucketName = awsSettings.bucketName,
                        Key = awsSettings.rootFolder + "/" + settings.selectedDefinition + "/" + fileName,
                        UploadId = initResponse.UploadId
                    };
                    await client.AbortMultipartUploadAsync(abortMPURequest);
                    throw exception;
                }
            }
        }
        public override async Task DeleteFile(string id)
        {
            var awsSettings = EditorSettingsLoader.Get<AmazonSettingsData>();
            var baseLink = GetAmazonDomainLink();
            var key = id.Replace(baseLink, "");
            using (IAmazonS3 client = new AmazonS3Client(awsSettings.accessKeyId, awsSettings.secretAccessKey, RegionEndpoint.GetBySystemName(ConvertEndpointToString(awsSettings.endpoint))))
            {
                DeleteObjectRequest request = new DeleteObjectRequest
                {
                    BucketName = awsSettings.bucketName,
                    Key = key,
                };
                await client.DeleteObjectAsync(request);
            }
        }
        public override Task<string> DownloadDataByName(string name)
        {
            var link = GetBaseLink() + name;
            return DownloadData(link);
        }
        public override async Task<List<FileMeta>> GetListFileMeta()
        {
            var resultList = new List<FileMeta>();
            var awsSettings = EditorSettingsLoader.Get<AmazonSettingsData>();
            var settings = EditorSettingsLoader.Get<SettingsData>();
            try
            {
                using (IAmazonS3 client = new AmazonS3Client(awsSettings.accessKeyId, awsSettings.secretAccessKey, RegionEndpoint.GetBySystemName(ConvertEndpointToString(awsSettings.endpoint))))
                {
                    ListObjectsRequest listObjectsRequest = new ListObjectsRequest
                    {
                        BucketName = awsSettings.bucketName,
                        Prefix = $"{awsSettings.rootFolder}/{settings.selectedDefinition}/",
                    };
                    ListObjectsResponse listResponse = await client.ListObjectsAsync(listObjectsRequest);
                    var link = GetAmazonDomainLink();
                    foreach (S3Object obj in listResponse.S3Objects)
                    {
                        resultList.Add(new FileMeta(obj.Key, link + obj.Key, obj.Key.Replace(listObjectsRequest.Prefix,""), obj.Size));
                    }
                }
                return resultList;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw e;
            }
        }
        public override async Task<string> GetFileIdByName(string name)
        {
            var link = GetBaseLink() + name;
            return link;
        }

        public override async Task<string> GetVersionsLink()
        {
            var link = GetBaseLink() + Consts.APP_DATA_FILE_NAME;
            return link;
        }
        public static string GetBaseLink()
        {
            var awsSettings = EditorSettingsLoader.Get<AmazonSettingsData>();
            var settings = EditorSettingsLoader.Get<SettingsData>();
            return  $"https://s3.{ConvertEndpointToString(awsSettings.endpoint)}.amazonaws.com/{awsSettings.bucketName}/{awsSettings.rootFolder}/{settings.selectedDefinition}/";
        }

        static string GetAmazonDomainLink()
        {
            var awsSettings = EditorSettingsLoader.Get<AmazonSettingsData>();
            return $"https://s3.{ConvertEndpointToString(awsSettings.endpoint)}.amazonaws.com/{awsSettings.bucketName}/";
        }
        static string GetBadAmazonDomainLink()
        {
            var awsSettings = EditorSettingsLoader.Get<AmazonSettingsData>();
            return $"https://{awsSettings.bucketName}.s3.{ConvertEndpointToString(awsSettings.endpoint)}.amazonaws.com/";
        }
    }
}
