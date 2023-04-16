// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules.Converters;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.API
{
    public class OsuAPI
    {
        private const string CLIENT_ID = "20064";
        private const string CLIENT_SECRET = "nHW8IEnoFRuXtjFMZxwpEMInlQ9klam5BCApCVC0";

        public const string BASE_URL = "https://osu.ppy.sh/api/v2"; 

        private static readonly SemaphoreSlim s_sempahore = new(1);

        private readonly HttpClient _httpClient = new();

        public static OsuAPI Instance { get; } = new();

        private OsuAPI() { }

        private async Task SetTokenAsync()
        {
            await s_sempahore.WaitAsync();
            string? refreshFileToken;
            using (StreamReader reader = new("RefreshToken.txt"))
            {
                refreshFileToken = await reader.ReadLineAsync();
            }
            if (refreshFileToken is null)
                throw new NotImplementedException("Пустой файл RefreshToken");
            KeyValuePair<string, string>[] contentValues = new KeyValuePair<string, string>[]
            {
                new ("client_id", CLIENT_ID),
                new ("client_secret", CLIENT_SECRET),
                new ("grant_type", "refresh_token"),
                new ("refresh_token", refreshFileToken)
            };
            FormUrlEncodedContent content = new(contentValues);
            using HttpResponseMessage response = await _httpClient.PostAsync("https://osu.ppy.sh/oauth/token", content);
            JToken jsonResponse = JToken.Parse(await response.Content.ReadAsStringAsync());
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jsonResponse?["access_token"]?.ToString());
            using (StreamWriter writer = new("RefreshToken.txt", false))
            {
                await writer.WriteLineAsync(jsonResponse?["refresh_token"]?.ToString());
                await writer.WriteLineAsync(jsonResponse?["access_token"]?.ToString());
            }
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

        private async Task<T?> GetAsync<T>(string url, string? pathParse = null)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, $"{BASE_URL}{url}");
            using HttpResponseMessage response = await SendAsync(request);
            string str = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (pathParse is null)
                    return JsonConvert.DeserializeObject<T>(str);
                else
                {
                    JObject jo = JObject.Parse(str);
                    return JsonConvert.DeserializeObject<T>(jo[pathParse].ToString());
                }
            }

            return default;
        }

        private async Task<T?> PostAsync<T>(string url, JObject json)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, $"{BASE_URL}{url}")
            {
                Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json")
            };
            using HttpResponseMessage response = await SendAsync(request);
            string str = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<T>(str);

            return default;
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

        public async Task<string?> GetReplayData(long id)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/scores/osu/{id}/download");
            using HttpResponseMessage response = await SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<OsuBeatmap?> GetBeatmapAsync(long id)
        {
            return await GetAsync<OsuBeatmap>($"/beatmaps/{id}");
        }

        public async Task<OsuBeatmap?> GetBeatmapAsync(string beatmaphash)
        {
            return await GetAsync<OsuBeatmap>($"/beatmaps/lookup?checksum={beatmaphash}");
        }

        public async Task<OsuBeatmapset?> GetBeatmapsetAsync(long id)
        {
            return await GetAsync<OsuBeatmapset>($"/beatmapsets/{id}");
        }

        public async Task<OsuBeatmapAttributes?> GetBeatmapAttributesAsync(long beatmapId, int mods = 0)
        {
            if (mods == NoMod.NUMBER)
                mods = 0;

            IEnumerable<string> m = ModsConverter.ToStrings(mods);
            JObject json = JObject.FromObject(new
            {
                mods = m,
                ruleset = "osu",
            });
            return await PostAsync<OsuBeatmapAttributes>($"/beatmaps/{beatmapId}/attributes", json);
        }

        public async Task<IList<OsuScore>?> GetUserScoresAsync(UserScoreQueryParameters parameters)
        {
            return await GetAsync<IList<OsuScore>>(parameters.GetQueryString());
        }

        public async Task<OsuScore?> GetUserBeatmapBestScoreAsync(long beatmapId, long userId)
        {
            return await GetAsync<OsuScore>($"/beatmaps/{beatmapId}/scores/users/{userId}", "score");
        }

        public async Task<IList<OsuScore>?> GetUserBeatmapAllScoresAsync(long beatmapId, long userId)
        {
            return await GetAsync<IList<OsuScore>>($"/beatmaps/{beatmapId}/scores/users/{userId}/all", "scores");
        }
    }
}
