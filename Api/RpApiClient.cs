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
        const string urlProgramBlock = @"https://api.radioparadise.com/api/play?chan=0&event={0}&bitrate=4&info=true";

        static HttpClient httpClient = new HttpClient();

        public async Task<ProgramBlock> GetProgramBlockAsync (int eventId)
        {
            var url = String.Format(urlProgramBlock, eventId);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };
                return JsonSerializer.Deserialize<ProgramBlock>(responseContent, options);
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
