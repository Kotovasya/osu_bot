// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Entites.Database;
using System.Reflection;
using osu_bot.Modules;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu_bot.API
{
    public enum OrdrVerificationMode
    {
        DevSuccess,
        DevFail,
        DevWSFail,
        Release
    }

    public class OrdrAPI
    {
        private const string DEVMODE_SUCCESS = "devmode_success";
        private const string DEVMODE_FAIL = "devmode_fail";
        private const string DEVMODE_WSFAIL = "devmode_wsfail";
        private const string RELEASEMODE = "B83bB8mY9mLsQqLM";

        private const string DEFAULT_RESOLUTION = "1280x720";
        private const int DEFAULT_SKIN_ID = 0;

        private readonly string _verificationKey;
        private readonly HttpClient _httpClient = new();

        public static OrdrAPI Instance { get; } = new(OrdrVerificationMode.DevSuccess);

        private OrdrAPI(OrdrVerificationMode mode)
        {
            _verificationKey = mode switch
            {
                OrdrVerificationMode.DevSuccess => DEVMODE_SUCCESS,
                OrdrVerificationMode.DevFail => DEVMODE_FAIL,
                OrdrVerificationMode.DevWSFail => DEVMODE_WSFAIL,
                OrdrVerificationMode.Release => RELEASEMODE,
                _ => ""
            };
        }

        public async Task<IList<ReplaySkin>?> GetAllSkinsAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("https://apis.issou.best/ordr/skins/?page=1&pageSize=452");
            string content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IList<ReplaySkin>>(content);
        }

        public async Task<JObject> SendRenderAsync(string username, ReplaySettings settings, MemoryStream replayDataStream)
        {
            using MultipartFormDataContent data = new();
            using StreamContent streamContent = new(replayDataStream);

            int skinId = settings.Skin is null ? DEFAULT_SKIN_ID : settings.Skin.Id;
            
            data.Add(streamContent, name: "replayFile");
            data.Add(new StringContent(DEFAULT_RESOLUTION), name: "resolution");
            data.Add(new StringContent(_verificationKey), name: "verificationKey");
            data.Add(new StringContent(username), name: "username");

            Type type = settings.GetType();
            IEnumerable<PropertyInfo> properties =
                type.GetProperties().Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() is null);
            string requestName;
            string? requestValue;
            foreach (PropertyInfo property in properties)
            {
                requestName = property.Name.FirstCharToLower();
                requestValue = property.GetValue(settings)?.ToString();
                if (requestValue is not null)
                {
                    if (requestName == "skin")
                        requestValue = skinId.ToString();
                    data.Add(new StringContent(requestValue), name: requestName);
                }
            }

            HttpResponseMessage response = await _httpClient.PostAsync("https://apis.issou.best/ordr/renders", data);
            string content = await response.Content.ReadAsStringAsync();

            return JObject.Parse(content);
        }
    }
}
