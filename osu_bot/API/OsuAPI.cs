// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu_bot.Entites;

namespace osu_bot.API
{
    public class OsuAPI
    {
        public const string BASE_URL = "https://osu.ppy.sh/api/v2"; 

        private static readonly SemaphoreSlim s_sempahore = new(1);

        private readonly HttpClient _httpClient = new HttpClient();

        public static OsuAPI Instance { get; } = new();

        private async Task SetTokenAsync()
        {
            await s_sempahore.WaitAsync();
            var json = new
            {
                client_id = 20064,
                client_secret = "nHW8IEnoFRuXtjFMZxwpEMInlQ9klam5BCApCVC0",
                grant_type = "client_credentials",
                scope = "public"
            };
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync("https://osu.ppy.sh/oauth/token", json);
            JToken jsonResponse = JToken.Parse(await response.Content.ReadAsStringAsync());
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jsonResponse["access_token"].ToString());
            s_sempahore.Release();
        }

        public async Task InitalizeAsync() => await SetTokenAsync();

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await SetTokenAsync();
                response = await _httpClient.SendAsync(request);
            }

            return response;
        }
        
        public async Task<JObject> GetJsonAsync(string url)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, url);
            using HttpResponseMessage response = await SendAsync(request);
            string str = await response.Content.ReadAsStringAsync();
            return JObject.Parse(str);
        }

        public async Task<JObject> PostJsonAsync(string url, JObject json)
        {
            string str = json.ToString();
            using HttpRequestMessage request = new(HttpMethod.Post, url)
            {
                Content = new StringContent(str, Encoding.UTF8, "application/json")
            };
            using HttpResponseMessage response = await SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }

        public async Task<OsuUser?> GetUserAsync(string username)
        {
            JObject json = await GetJsonAsync($"{BASE_URL}/users/{username}");
            if (json.ContainsKey("error"))
                return null;

            return JsonConvert.DeserializeObject<OsuUser>(json.ToString());
        }

        public async Task<OsuUser?> GetUserAsync(long id)
        {
            JObject json = await GetJsonAsync($"{BASE_URL}/users/{id}");
            if (json.ContainsKey("error"))
                return null;

            return JsonConvert.DeserializeObject<OsuUser>(json.ToString());
        }

        public async Task<OsuBeatmap?> GetBeatmapAsync()
        {

        }
    }
}
