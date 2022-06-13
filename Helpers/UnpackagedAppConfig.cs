using Microsoft.Extensions.Configuration;
using RadioParadisePlayer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RadioParadisePlayer.Helpers
{
    class UnpackagedAppConfig : AppConfig
    {
        IConfiguration localSettings;

        public UnpackagedAppConfig()
        {
            var builder = 
                new ConfigurationManager()
                .SetBasePath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
                .AddJsonFile("settings.json");
            localSettings = builder.Build();
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
                default:
                    return value;
            };
        }
        
        public override T ReadValue<T>(string key)
        {
            if (localSettings[key] is null) return default;
            else return (T)ConvertString<T>(localSettings[key]);
        }

        public override T ReadValue<T>(string key, T defaultValue)
        {
            if (localSettings[key] is null) return defaultValue;
            else return (T)ConvertString<T>(localSettings[key]);
        }

        public override void WriteValue<T> (string key, T value)
        {
            localSettings[key] = Convert.ToString(value);
        }
    }
}
