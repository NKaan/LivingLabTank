

namespace SIDGIN.Patcher.Client
{
    using Common;
    public static class FileEntryExtensions
    {
        public static string ToXML(this PatchEntry data)
        {
            return XmlSerializeHelper.Serialize(data);
        }
        internal static PatchEntry ToPatchEntry(this string data)
        {
            return XmlSerializeHelper.Deserialize<PatchEntry>(data);
        }
    }
}
