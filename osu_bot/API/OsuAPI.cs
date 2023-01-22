using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace osu_bot.API
{
    public class OsuAPI
    {
        private readonly HttpClient httpClient = new();

        private async Task SetTokenAsync()
        {
            var json = new
            {
                client_id = 20064,
                client_secret = "nHW8IEnoFRuXtjFMZxwpEMInlQ9klam5BCApCVC0",
                grant_type = "client_credentials",
                scope = "public"
            };
            using var response = await httpClient.PostAsJsonAsync("https://osu.ppy.sh/oauth/token", json);
            JObject jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jsonResponse["access_token"].ToString());
        }

        public async Task InitalizeAsync()
        {
            await SetTokenAsync();
        }

        //Здесь нужно начать различать массив и обычные объекты
        public async Task<JObject> GetJsonAsync(string url)
        {
            using var response = await httpClient.GetAsync(url);
            var str = await response.Content.ReadAsStringAsync();
            return JObject.Parse(str);
        }

        public async Task<JArray> GetJsonArrayAsync(string url)
        {
            using var response = await httpClient.GetAsync(url);
            var str = await response.Content.ReadAsStringAsync();
            return JArray.Parse(str);
        }

        public async Task<JObject> PostJsonAsync(string url, JObject json)
        {
            using var response = await httpClient.PostAsJsonAsync(url, json);
            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<int> GetIdByUsernameAsync(string username)
        {
            var json = await GetJsonAsync($"https://osu.ppy.sh/api/v2/users/{username}");
            return int.Parse(json["id"].ToString());
        }
    }
}
