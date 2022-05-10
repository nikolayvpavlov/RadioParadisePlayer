using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Api
{
    internal class ProgramBlock
    {
        public int Event { get; set; }
        public long Sched_Time_Millis { get; set; }
        public string Type { get; set; }
        public string End_Event { get; set; }
        public string Length { get; set; }
        public string Url { get; set; }
        public int Timeout_Millis { get; set; }
        public string Chan { get; set; }
        public Channel Channel { get; set; }
        public string Bitrate { get; set; }
        public string Ext { get; set; }
        public int Cue { get; set; }
        public int Expiration { get; set; }
        //public string Filename { get; set; } //Ignore for now
        public string Image_Base { get; set; }
        [JsonPropertyName("song")]
        public Dictionary<int, Song> Songs { get; set; }
        public Dictionary<int, CacheAheadTimeout> Cache_Ahead_Timeout { get; set; }
        public int Current_Event_Id { get; set; }
        public int Max_Gapless_Event_Id { get; set; }
    }
}
