using SIDGIN.Zip.Core;
using SIDGIN.Zip;
using System;
using System.IO;
using SIDGIN.Patcher.Client;
namespace SIDGIN.Patcher.Common
{
    internal static class ArchiveHelper
    {
        static readonly int bufferSize = 4096;

        public static Stream ExtractStream(ZipFile zf, string zipFilepath)
        {
            ZipEntry zipEntry = zf.GetEntry(zipFilepath);
            return zf.GetInputStream(zipEntry);
        }
        public static string ExtractData(ZipFile zf, string zipFilepath)
        {
            ZipEntry zipEntry = zf.GetEntry(zipFilepath);
            Stream zipStream = zf.GetInputStream(zipEntry);

            byte[] buffer = new byte[zipEntry.Size];
            zipStream.Read(buffer, 0, buffer.Length);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }
        public static void ExtractFile(ZipFile zf,string filepath,string zipFilepath)
        {
            ZipEntry zipEntry = zf.GetEntry(zipFilepath);
            Stream zipStream = zf.GetInputStream(zipEntry);

            if (!Directory.Exists(Path.GetDirectoryName(filepath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            byte[] buffer = new byte[bufferSize];

            using (FileStream streamWriter = File.Create(filepath))
            {
                streamWriter.Flush();
                StreamUtils.Copy(zipStream, streamWriter, buffer);
                streamWriter.Close();
            }
        }

        public static void ExtractZip(string zipFilepath, string output, Action<float,string> callback)
        {
            callback?.Invoke(0, Localize.Get("Read"));
            using (FileStream fs = File.OpenRead(zipFilepath))
            {
                using (ZipFile zf = new ZipFile(fs))
                {

                    for (int i = 0; i < zf.Count; i++)
                    {
                        ZipEntry zipEntry = zf[i];
                        Stream zipStream = zf.GetInputStream(zipEntry);

                        var outputFilepath = Path.Combine(output, zipEntry.ToString());
                        if (!Directory.Exists(Path.GetDirectoryName(outputFilepath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(outputFilepath));

                        byte[] buffer = new byte[bufferSize];

                        using (FileStream streamWriter = File.Create(outputFilepath))
                        {
                            streamWriter.Flush();
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }

                        float progress = (float)i / zf.Count;
                        callback?.Invoke(progress, Localize.Format("Extract", zipEntry.ToString()));
                    }


                    zf.Close();
                }
                fs.Close();
            }
            callback?.Invoke(1, Localize.Get("Extract_completed"));
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

            byte[] buffer = new byte[bufferSize];
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
