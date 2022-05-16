using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Api
{
    internal class Song
    {
        public int Song_Id { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public int Duration { get; set; }
        public string Album { get; set; }
        public int User_Rating { get; set; }
        public double Rating { get; set; }
        public string Cover_Art { get; set; }
        public int Event_Id { get; set; }
        public string Gapless_Url { get; set; }
        public List<string> Slideshow { get; set; }
        public int Cue { get; set; }
        public string Type { get; set; }
    }
}
