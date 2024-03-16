using System;
using System.Xml.Serialization;
namespace SIDGIN.Patcher.Client
{
    [System.Serializable]
    public struct Version : IComparable<Version>
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }

        public static bool operator > (Version ver1, Version ver2)
        {
            if (ver1.Major > ver2.Major) return true;
            if (ver1.Major < ver2.Major) return false;
            if (ver1.Minor > ver2.Minor) return true;
            if (ver1.Minor < ver2.Minor) return false;
            if (ver1.Build > ver2.Build) return true;
            if (ver1.Build < ver2.Build) return false;
            if (ver1.Revision > ver2.Revision) return true;
            if (ver1.Revision < ver2.Revision) return false;
            return false;
        }
        public static bool operator < (Version ver1, Version ver2)
        {
            if (ver1.Major > ver2.Major) return false;
            if (ver1.Major < ver2.Major) return true;
            if (ver1.Minor > ver2.Minor) return false;
            if (ver1.Minor < ver2.Minor) return true;
            if (ver1.Build > ver2.Build) return false;
            if (ver1.Build < ver2.Build) return true;
            if (ver1.Revision > ver2.Revision) return false;
            if (ver1.Revision < ver2.Revision) return true;
            return false;
        }
        public static Version operator + (Version ver, int revision)
        {
            ver.Revision+=revision;
            return ver;
        }
        public static Version operator ++ (Version ver)
        {
            ver.Revision++;
            return ver;
        }
        public static Version operator -- (Version ver)
        {
            ver.Revision--;
            return ver;
        }
        public static bool operator == (Version ver1, Version ver2)
        {

            return ver1.Major == ver2.Major&&    
            ver1.Minor == ver2.Minor&&
            ver1.Build == ver2.Build&&
            ver1.Revision == ver2.Revision;
        }
        public static bool operator != (Version ver1, Version ver2)
        {
            return ver1.Major != ver2.Major ||
        ver1.Minor != ver2.Minor ||
        ver1.Build != ver2.Build ||
        ver1.Revision != ver2.Revision;
        }
        public static bool operator >= (Version ver1, Version ver2)
        {
            return ver1 == ver2 || ver1 > ver2;
        }
        public static bool operator <= (Version ver1, Version ver2)
        {
            return ver1 == ver2 || ver1 < ver2;
        }
        public int CompareTo(Version that)
        {
            if (this.Major > that.Major)return -1;
            if (this.Major < that.Major) return 1;
            if (this.Minor > that.Minor) return -1;
            if (this.Minor < that.Minor) return 1;
            if (this.Build > that.Build) return -1;
            if (this.Build < that.Build) return 1;
            if (this.Revision > that.Revision) return -1;
            if (this.Revision < that.Revision) return 1;
            if (this.Revision == that.Revision) return 0;
            return 0;
        }
        /// <summary>
        /// Convert Version to string
        /// </summary>
        /// <returns>string format XXXX.XXXX.XXXX.XXXX</returns>
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}.{Revision}";
        }
        /// <summary>
        /// Convert Version to string
        /// </summary>
        /// <returns>string format XXXX_XXXX_XXXX_XXXX</returns>
        public string ToPatchFormatString()
        {
            return $"{Major}_{Minor}_{Build}_{Revision}";
        }

        public override bool Equals(object obj)
        {
            var version = (Version)obj;
            return version != null &&
                   Major == version.Major &&
                   Minor == version.Minor &&
                   Build == version.Build &&
                   Revision == version.Revision;
        }

        public override int GetHashCode()
        {
            var hashCode = -1452750829;
            hashCode = hashCode * -1521134295 + Major.GetHashCode();
            hashCode = hashCode * -1521134295 + Minor.GetHashCode();
            hashCode = hashCode * -1521134295 + Build.GetHashCode();
            hashCode = hashCode * -1521134295 + Revision.GetHashCode();
            return hashCode;
        }
        public static Version Empty
        {
            get
            {
                return new Version();
            }
        }
    }
  
}
