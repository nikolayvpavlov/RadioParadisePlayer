using RadioParadisePlayer.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace RadioParadisePlayer.Helpers
{
    class UnpackagedAppConfig : AppConfig
    {
        Dictionary<string, string> localSettings = new Dictionary<string, string>();
        string settingsFileName;

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.Strict | JsonNumberHandling.AllowReadingFromString
        };

    public UnpackagedAppConfig()
        {
            settingsFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\RadioParadise Player\\settings.json";
            try
            {
                string settingsJson = File.ReadAllText(settingsFileName);
                localSettings = JsonSerializer.Deserialize<Dictionary<string, string>>(settingsJson, jsonOptions);
            }
            catch
            {
                localSettings = new();
            }
        }

        private string getKey(string key)
        {
            if (localSettings.TryGetValue(key, out string result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private object ConvertString<T>(string value)
        {
            T t = default;
            switch (t)
            {
                case int iNum:
                    return int.Parse(value);
                case double dNum:
                    return double.Parse(value);
                case bool b:
                    return bool.Parse(value);
                default:
                    return value;
            };
        }

        public override T ReadValue<T>(string key)
        {
            string value = getKey(key);
            if (value is null) return default;
            else return (T)ConvertString<T>(value);
        }

        public override T ReadValue<T>(string key, T defaultValue)
        {
            string value = getKey(key);
            if (value is null) return defaultValue;
            else return (T)ConvertString<T>(value);
        }

        public override void WriteValue<T>(string key, T value)
        {
            if (localSettings.ContainsKey(key))
            {
                localSettings[key] = value.ToString();
            }
            else
            {
                localSettings.Add(key, value.ToString());
            }
            var json = System.Text.Json.JsonSerializer.Serialize<Dictionary<string, string>>(localSettings);
            File.WriteAllText(settingsFileName, json);
        }
    }
}
