// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using osu_bot.Entites;

namespace osu_bot.API
{
    public class OsuAPI
    {
        private readonly HttpClient httpClient = new();

        public static OsuAPI Instance { get; } = new();

        private async Task SetTokenAsync()
        {
            var json = new
            {
                client_id = 20064,
                client_secret = "nHW8IEnoFRuXtjFMZxwpEMInlQ9klam5BCApCVC0",
                grant_type = "client_credentials",
                scope = "public"
            };
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("https://osu.ppy.sh/oauth/token", json);
            JToken jsonResponse = JToken.Parse(await response.Content.ReadAsStringAsync());
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jsonResponse["access_token"].ToString());
        }

        private async Task<bool> CheckValidTokenAsync(string content)
        {
            bool valid = !content.Contains("authentication");
            if (!valid)
                await SetTokenAsync();
            return valid;
        }

        public async Task InitalizeAsync() => await SetTokenAsync();

        public async Task<JToken> GetJsonAsync(string url)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url);            
            string str = await response.Content.ReadAsStringAsync();
            if (!await CheckValidTokenAsync(str))
                return await GetJsonAsync(url);
            return JToken.Parse(str);
        }

        public async Task<JArray> GetJsonArrayAsync(string url)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            string str = await response.Content.ReadAsStringAsync();
            if (!await CheckValidTokenAsync(str))
                return await GetJsonArrayAsync(url);
            return JArray.Parse(str);
        }

        public async Task<JToken> PostJsonAsync(string url, JToken json)
        {
            string str = json.ToString();
            using HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(str, Encoding.UTF8, "application/json"));
            string content = await response.Content.ReadAsStringAsync();
            if (!await CheckValidTokenAsync(content))
                return await PostJsonAsync(url, json);
            return JToken.Parse(content);
        }

        public async Task<OsuUser> GetUserInfoByUsernameAsync(string username)
        {
            JToken json = await GetJsonAsync($"https://osu.ppy.sh/api/v2/users/{username}");
            if (!await CheckValidTokenAsync(json.ToString()))
                return await GetUserInfoByUsernameAsync(username);
            if (json.Contains("error"))
            {
                throw new ArgumentException($"Пользователь с именем {username} не зарегистрирован");
            }

            OsuUser user = new();
            user.ParseUserJson(json);
            return user;
        }

        public async Task<byte[]> BeatmapsetDownload(long beatmapsetId)
        {
            return await httpClient.GetByteArrayAsync($"https://osu.ppy.sh/beatmapsets/{beatmapsetId}/download");
        }
    }
}
