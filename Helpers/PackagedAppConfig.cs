using RadioParadisePlayer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RadioParadisePlayer.Helpers
{
    class PackagedAppConfig : AppConfig
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        
        public override T ReadValue<T>(string key)
        {
            if (localSettings.Values[key] is null) return default;
            else return (T)localSettings.Values[key];
        }

        public override T ReadValue<T>(string key, T defaultValue)
        {
            if (localSettings.Values[key] is null) return defaultValue;
            else return (T)localSettings.Values[key];
        }

        public override void WriteValue<T> (string key, T value)
        {
            localSettings.Values[key] = value;
        }
    }
}
