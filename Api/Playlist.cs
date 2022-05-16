using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Api
{
    internal class Playlist
    {
        public Channel Channel{ get; set; }
        public string Bitrate_Title { get; set; }
        public string Extension { get; set; }
        [JsonPropertyName("imgage_base")]
        public string Image_Base { get; set; }
        public int Current_Event_Id { get; set; }
        public int Max_Gapless_Event_Id { get; set; }
        public string Slideshow_Path { get; set; }
        public List<Song> Songs { get; set; }
    }
}
