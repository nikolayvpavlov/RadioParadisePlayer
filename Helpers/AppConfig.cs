using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Helpers
{
    public abstract class AppConfig
    {
        public abstract T ReadValue<T>(string key);

        public abstract T ReadValue<T>(string key, T defaultValue);

        public abstract void WriteValue<T>(string key, T value);
    }
}
