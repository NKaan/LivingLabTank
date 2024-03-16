using SIDGIN.Common.Editors;
using SIDGIN.Zip;
using SIDGIN.Zip.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace SIDGIN.Patcher.Editors
{
    public static class CompressHelper
    {
        public static void CompressFiles(List<string> files, string fileOutput, Action<float, string> callback)
        {
            callback?.Invoke(0, "Package creation...");
            var settingsData = EditorSettingsLoader.Get<SettingsData>();
            var versionDirectory = Path.Combine(EditorConsts.VERSIONS_PATH, settingsData.patchTask.version);
            using (FileStream fsOut = File.Create(fileOutput))
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
                {
                    zipStream.SetLevel(settingsData.zipCompressLevel);
                    for (int i = 0; i < files.Count; i++)
                    {

                        var filepath = Path.Combine(versionDirectory, files[i]);
                        var progressHandler = new ProgressHandler((o, e) =>
                        {
                            if (e.Target != 0)
                                callback?.Invoke((float)(i - 1 + (e.Processed / e.Target * 0.01f)) / files.Count, $"File compression {files[i]}");
                            else
                                callback?.Invoke(0, $"File compression {files[i]}");
                        });
                        FileInfo fi = new FileInfo(filepath);

                        ZipEntry newZipEntry = new ZipEntry(files[i]);
                        newZipEntry.Size = fi.Length;

                        zipStream.PutNextEntry(newZipEntry);

                        byte[] buffer = new byte[4096];
                        using (FileStream streamReader = File.OpenRead(filepath))
                        {
                            StreamUtils.Copy(streamReader, zipStream, buffer, progressHandler, new TimeSpan(0, 0, 5), null, "", newZipEntry.Size);
                        }
                        zipStream.CloseEntry();

                    }
                    zipStream.IsStreamOwner = true;
                    zipStream.Close();
                }
            }

            callback?.Invoke(1, "Package creation complete.");
        }
       
        public static void CompressFile(ZipOutputStream zipStream, string filepath, string zipFilepath, Action<float> onProgress)
        {
            var progressHandler = new ProgressHandler((o, e) =>
            {
                if (e.Target != 0)
                    onProgress(e.Processed / e.Target * 0.01f);
                else
                    onProgress(0);
            });
            FileInfo fi = new FileInfo(filepath);

            ZipEntry newZipEntry = new ZipEntry(zipFilepath);
            newZipEntry.Size = fi.Length;

            zipStream.PutNextEntry(newZipEntry);

            byte[] buffer = new byte[4096];
            using (FileStream streamReader = File.OpenRead(filepath))
            {
                StreamUtils.Copy(streamReader, zipStream, buffer, progressHandler, new TimeSpan(0, 0, 5), null, "", newZipEntry.Size);
            }
            zipStream.CloseEntry();
        }

        public static void CompressFileFromData(ZipOutputStream zipStream, string zipFilepath, string data)
        {
            CompressFileFromData(zipStream, zipFilepath, System.Text.Encoding.UTF8.GetBytes(data));
        }

        public static void CompressFileFromData(ZipOutputStream zipStream, string zipFilepath, byte[] data)
        {
            ZipEntry newZipEntry = new ZipEntry(zipFilepath);
            newZipEntry.Size = data.Length;

            zipStream.PutNextEntry(newZipEntry);

            zipStream.Write(data, 0, data.Length);

            zipStream.CloseEntry();
        }
    }
}
