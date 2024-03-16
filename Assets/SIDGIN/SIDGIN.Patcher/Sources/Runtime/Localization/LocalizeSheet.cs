using SIDGIN.Patcher.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIDGIN.Patcher.Client
{
    [System.Serializable]
    public class LocalizeSheet
    {
        [System.Serializable]
        public class LocalizeKey
        {
            public string Key;
            public string Value;
        }
        
        public List<LocalizeKey> localizeKeys;

        public Dictionary<string, string> Load()
        {
            var keys = new Dictionary<string, string>();
            foreach (var item in localizeKeys)
            {
                keys.Add(item.Key, item.Value);
            }
            return keys;
        }
        public static void Save(string path, Dictionary<string, string> sheet)
        {
            if (sheet == null)
                return;
            var localizeSheet = new LocalizeSheet
            {
                localizeKeys = sheet.Select(x =>
                new LocalizeSheet.LocalizeKey { Key = x.Key, Value = x.Value }).ToList()
            };
            var stringData = XmlSerializeHelper.Serialize(localizeSheet);
            File.WriteAllText(path, stringData);
        }
    }
}
