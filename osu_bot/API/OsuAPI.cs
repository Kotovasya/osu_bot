// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http.Json;
using System.Text;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules.Converters;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.API
{
    public class OsuAPI
    {
        public const string BASE_URL = "https://osu.ppy.sh/api/v2"; 

        private static readonly SemaphoreSlim s_sempahore = new(1);

        private readonly HttpClient _httpClient = new();

        private readonly DatabaseContext _database = DatabaseContext.Instance;

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

        private async Task<T?> GetAsync<T>(string url)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, $"{BASE_URL}{url}");
            using HttpResponseMessage response = await SendAsync(request);
            JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (json.ContainsKey("error"))
                return default;
            return JsonConvert.DeserializeObject<T>(json.ToString());
        }

        private async Task<T?> PostAsync<T>(string url, JObject json)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, $"{BASE_URL}{url}")
            {
                Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json")
            };
            using HttpResponseMessage response = await SendAsync(request);
            JObject jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (json.ContainsKey("error"))
                return default;
            return JsonConvert.DeserializeObject<T>(jsonResponse.ToString());
        }

        public async Task<OsuUser?> GetUserAsync(string username)
        {
            return await GetAsync<OsuUser>($"/users/{username}");
        }

        public async Task<OsuUser?> GetUserAsync(long id)
        {
            return await GetAsync<OsuUser>($"/users/{id}");
        }


        public async Task<OsuScore?> GetScoreAsync(long id)
        {
            return await GetAsync<OsuScore>($"/scores/osu/{id}");
        }

        public async Task<OsuBeatmap?> GetBeatmapAsync(long id)
        {
            return await GetAsync<OsuBeatmap>($"/beatmaps/{id}");
        }

        public async Task<OsuBeatmapset?> GetBeatmapsetAsync(long id)
        {
            return await GetAsync<OsuBeatmapset>($"/beatmapsets/{id}");
        }

        public async Task<OsuBeatmapAttributes?> GetBeatmapAttributesAsync(long beatmapId, int mods = 0)
        {
            JObject json = JObject.FromObject(new
            {
                mods = mods,
                ruleset = "osu",
            });
            return await PostAsync<OsuBeatmapAttributes>($"/beatmaps/{beatmapId}/attributes", json);
        }

        public async Task<IList<OsuScore>?> GetUserScoresAsync(UserScoreQueryParameters parameters)
        {
            IList<OsuScore>? scores = await GetAsync<IList<OsuScore>>(parameters.GetQueryString());
            if (scores is null)
                return null;

            return scores.Where(s => s.Mods == parameters.Mods)
                .Skip(parameters.Offset)
                .Take(parameters.Limit)
                .ToList();
        }

        public async Task<OsuScore?> GetUserBeatmapBestScoreAsync(long userId, long beatmapId)
        {
            return await GetAsync<OsuScore>($"/beatmaps/{beatmapId}/scores/users/{userId}");
        }

        public async Task<IList<OsuScore>?> GetUserBeatmapAllScoresAsync(long beatmapId, long userId)
        {
            return await GetAsync<IList<OsuScore>>($"/beatmaps/{beatmapId}/scores/users/{userId}/all");
        }
    }
}
