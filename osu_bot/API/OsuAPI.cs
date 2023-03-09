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

        public async Task InitalizeAsync() => await SetTokenAsync();

        //Здесь нужно начать различать массив и обычные объекты
        public async Task<JToken> GetJsonAsync(string url)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            string str = await response.Content.ReadAsStringAsync();
            return JToken.Parse(str);
        }

        public async Task<JArray> GetJsonArrayAsync(string url)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            string str = await response.Content.ReadAsStringAsync();
            return JArray.Parse(str);
        }

        public async Task<JToken> PostJsonAsync(string url, JToken json)
        {
            string str = json.ToString();
            using HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(str, Encoding.UTF8, "application/json"));
            //using var response = await httpClient.PostAsJsonAsync(url, str);
            string content = await response.Content.ReadAsStringAsync();
            return JToken.Parse(content);
        }

        public async Task<User> GetUserInfoByUsernameAsync(string username)
        {
            JToken json = await GetJsonAsync($"https://osu.ppy.sh/api/v2/users/{username}");
            if (json.Contains("error"))
            {
                throw new ArgumentException($"Пользователь с именем {username} не зарегистрировано");
            }

            User user = new();
            user.ParseUserJson(json);
            return user;
        }
    }
}
