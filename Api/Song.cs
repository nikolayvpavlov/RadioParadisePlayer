using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Api
{
    internal class Song
    {
        public int Event { get; set; }
        public string Type { get; set; }
        public long Sched_Time_Millis { get; set; }
        public string Song_Id { get; set; }
        public int Duration { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        public string Asin { get; set; }
        public string Rating { get; set; }
        public string Slideshow { get; set; }
        public object User_Rating { get; set; }
        public string Cover { get; set; }
        public int Elapsed { get; set; }
        public int Timeout_Millis { get; set; }
        public string Gapless_Url { get; set; }
        public int Cache_Length_Millis_Max { get; set; }
    }
}
