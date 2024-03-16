using SIDGIN.Patcher.Common;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace SIDGIN.Patcher.Client
{
    public static class Localize
    {
        static Dictionary<string, string> keys = new Dictionary<string, string>();
        const string defaultLanguageKey = "English";
        public static void Load()
        {
            if (keys.Count > 0)
                return;
            var clientSettings = ClientSettings.Get();
            var localizeDirectory = Path.Combine(Application.persistentDataPath, "loc");
            if (Directory.Exists(localizeDirectory))
            {
                if (string.IsNullOrEmpty(clientSettings.languageKey))
                {
                    LoadFromSystemOrDefault(localizeDirectory);
                }
                else
                {
                    var languageFile = Path.Combine(localizeDirectory, $"{clientSettings.languageKey}.xml");
                    if (!File.Exists(languageFile))
                    {
                        LoadFromSystemOrDefault(localizeDirectory);
                        return;
                    }
                    var stringData = File.ReadAllText(languageFile);
                    var languageSheet = XmlSerializeHelper.Deserialize<LocalizeSheet>(stringData);
                    if (languageSheet != null)
                    {
                        var _keys = languageSheet.Load();
                        if (_keys != null && _keys.Count > 0)
                            keys = _keys;
                    }
                    else
                    {
                        LoadFromSystemOrDefault(localizeDirectory);
                    }
                }
            }
        }
      
        static void LoadFromSystemOrDefault(string dir)
        {
            var systemLanguageKey = defaultLanguageKey;
            if (CultureInfo.CurrentUICulture != null)
            {
                if (CultureInfo.CurrentUICulture.IsNeutralCulture)
                {
                    systemLanguageKey = CultureInfo.CurrentUICulture.EnglishName;
                }
                else if(CultureInfo.CurrentUICulture.Parent != null)
                {
                    systemLanguageKey = CultureInfo.CurrentUICulture.Parent.EnglishName;
                }
            }
            var systemLanguageFile = Path.Combine(dir, $"{systemLanguageKey}.xml");
            if (File.Exists(systemLanguageFile))
            {
                var stringData = File.ReadAllText(systemLanguageFile);
                Load(stringData);
            }
            else
            {
                var defaultLanguageFile = Path.Combine(dir, $"{defaultLanguageKey}.xml");
                if (!File.Exists(defaultLanguageFile))
                {
                    return;
                }
                var stringData = File.ReadAllText(defaultLanguageFile);
                Load(stringData);
            }
        }
        public static void Load(Dictionary<string,string> sheet)
        {
            if(sheet != null && sheet.Count > 0)
            {
                keys = sheet;
            }
        }
        public static void Load(string sheetData)
        {
            var languageSheet = XmlSerializeHelper.Deserialize<LocalizeSheet>(sheetData);
            if (languageSheet != null)
            {
                var _keys = languageSheet.Load();
                if (_keys != null && _keys.Count > 0)
                    keys = _keys;
            }
        }
        static string GetFromFile(string key)
        {
            if (keys.TryGetValue(key, out string result))
            {
                return result;
            }
            return "";
        }
        public static string Get(string key)
        {
            var value = GetFromFile(key);
            if (string.IsNullOrEmpty(value))
            {
                if (LocalizationDefault.Keys.TryGetValue(key, out string result))
                {
                    return result;
                }
                return key;
            }
            return value;
        }
        public static string Format(string key, params string[] parameters)
        {
            var text = Get(key);
            if (text.Contains("{0}"))
            {
                return string.Format(text, parameters);
            }
            else
            {
                return text;
            }
        }
    }
}
