
using System.IO;
using System.Xml.Serialization;

namespace SIDGIN.Patcher.Common
{
    public static class XmlSerializeHelper
    {

        public static T Deserialize<T>(string data)
        {
            return (T)Deserialize(data, typeof(T));
        }
        public static object Deserialize(string data, System.Type type)
        {
            var serializer = new XmlSerializer(type);
            object result = null;

            using (TextReader reader = new StringReader(data))
            {
                result = serializer.Deserialize(reader);
            }
            return result;
        }
        public static string Serialize(object data)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(data.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, data);
                return textWriter.ToString();
            }
        }

    }
}
