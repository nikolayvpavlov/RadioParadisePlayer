using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Api
{
    internal class SongInfo
    {
        [JsonPropertyName("wiki_html")]
        public string WikiHtml { get; set; }

        [JsonPropertyName("avg_rating")]
        public double AverageRating { get; set; }

        public string Lyrics { get; set; }
    }
}
