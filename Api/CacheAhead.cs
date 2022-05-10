using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Api
{
    internal class CacheAheadTimeout
    {
        public long Cache_Length_Millis_Max { get; set; }
        public int Timeout_Millis { get; set; }
    }
}
