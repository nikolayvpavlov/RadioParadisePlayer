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
        const string urlPlaylist = @"https://api.radioparadise.com/api/gapless";//?C_user_id={0}&player_id={1}&chan={2}&bitrate={3}";
        const string urlAuth = @"https://api.radioparadise.com/api/auth";

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

        public static async Task<Playlist> GetPlaylistAsync()
        {
            var url = String.Format(urlPlaylist);
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
            await stream.CopyToAsync (result);
            result.Position = 0;
            return result;
        }
        
    }
}
