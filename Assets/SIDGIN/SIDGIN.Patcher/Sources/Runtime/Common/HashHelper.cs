using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SIDGIN.Patcher.Common
{
    public static class HashHelper
    {
        public static string GetHashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
