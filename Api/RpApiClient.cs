using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Api
{
    class RpApiClient
    {
        const string urlPlaylist = @"https://api.radioparadise.com/api/gapless?C_user_id={0}&player_id={1}&chan={2}&bitrate={3}&source={4}";
        const string urlAuth = @"https://api.radioparadise.com/api/auth";
        const string urlChannels = @"https://api.radioparadise.com/api/list_chan?C_user_id={0}";
        const string urlSongStarts = @"https://api.radioparadise.com/api/update_history?song_id={0}&chan{1}&source{2}&player_id={3}&event={4}";
        const string urlSongPauses = @"https://api.radioparadise.com/api/update_pause?pause={0}&player_id={1}&event={2}&chan{3}&source{4}";
        const string urlGetSongInfo = @"https://api.radioparadise.com/siteapi.php?file=music::song&withWiki=true&song_id={0}&C_user_id={1}";

        const string PlayerId = "{2015FABE-E98E-4071-8232-57494B06D73B}";
        const int SourceId = 30; //Constant provided by Jarred (RP)

        static HttpClient httpClient = new HttpClient();

        static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };

        public static async Task<User> AuthenticateAsync()
        {
            var response = await httpClient.GetAsync(urlAuth);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(responseContent, jsonOptions);
            }
            else return null;
        }

        public static async Task<Playlist> GetPlaylistAsync(string userId, string channel, string bitrate)
        {
            var url = String.Format(urlPlaylist, userId, PlayerId, channel, bitrate, SourceId);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Playlist>(responseContent, jsonOptions);
            }
            else return null;
        }

        public static async Task<MemoryStream> DownloadImageAsync(string url)
        {
            var stream = await httpClient.GetStreamAsync(url);
            MemoryStream result = new MemoryStream();
            await stream.CopyToAsync(result);
            result.Position = 0;
            return result;
        }

        public static async Task<IReadOnlyList<Channel>> GetChannelsAsync(string userId)
        {
            var url = String.Format(urlChannels, userId);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Channel>>(responseContent, jsonOptions);
            }
            else return null;
        }

        public static async Task NotifyServiceSongStarts(Song song, string channel)
        {
            var url = String.Format(urlSongStarts, song.Song_Id, SourceId, channel, PlayerId, song.Event_Id);
            try
            {
                var response = await httpClient.GetAsync(url);
            }
            catch
            {
                //Fail silently.
            }
        }

        public static async Task NotifyServiceSongPause(int positionMs, Song song, string channel)
        {
            var url = String.Format(urlSongPauses, positionMs, PlayerId, song.Event_Id, channel, SourceId);
            try
            {
                var response = await httpClient.GetAsync(url);
            }
            catch
            {
                //Fail silently.
            }
        }

        public static async Task<SongInfo> GetSongInfoAsync(Song song, string userId)
        {
            var url = String.Format(urlGetSongInfo, song.Song_Id, userId);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SongInfo>(responseContent, jsonOptions);
            }
            else return null;
        }
    }
}
