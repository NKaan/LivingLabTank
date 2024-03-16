using System;
namespace SIDGIN.Patcher.Client
{
    public static class VersionExtensions
    {
        /// <summary>
        /// Convert Version to string
        /// </summary>
        /// <returns>string format XXXX.XXXX.XXXX.XXXX</returns>
        /// <summary>
        public static Version ToVersion(this string str)
        {
            var numbers = str.Split('.');
            if (numbers.Length != 4)
                throw new Exception("Conversion is not possible: incorrect string format");


            int major = 0;
            if (!int.TryParse(numbers[0], out major))
                throw new Exception("Conversion is not possible: Major value is incorrect");

            int minor = 0;
            if (!int.TryParse(numbers[1], out minor))
                throw new Exception("Conversion is not possible: Minor value is incorrect");

            int build = 0;
            if (!int.TryParse(numbers[2], out build))
                throw new Exception("Conversion is not possible: Build value is incorrect");

            int revision = 0;
            if (!int.TryParse(numbers[3], out revision))
                throw new Exception("Conversion is not possible: Revision value is incorrect");

            return new Version() { Major = major, Minor = minor, Build = build, Revision = revision };
        }


    }
}
