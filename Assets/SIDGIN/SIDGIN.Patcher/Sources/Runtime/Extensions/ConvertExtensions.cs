using System;
using System.IO;
using System.Text;

namespace SIDGIN.Patcher.Client
{
    public static class ConvertExtensions
    {
        public static string NormalizeFileSize(this long bytes)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB" };
            int place = 0;
            if (bytes > 0)
                place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 2);
            return string.Format("{0:#.00}", num) + " " + suf[place];
        }
        public static Stream StringToStream(this string data)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            MemoryStream stream = new MemoryStream(byteArray);
            return stream;
        }
        public static string StreamToString(this Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
