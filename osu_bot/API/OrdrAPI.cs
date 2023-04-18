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

namespace osu_bot.API
{
    public class OrdrAPI
    {
        private const string BOT_NAME = "KS!Bot";
        private const string DEFAULT_RESOLUTION = "1280x720";
        private const int DEFAULT_SKIN = 0;
        public string? DefaultVerificationKey { get; set; }

        private readonly HttpClient _httpClient = new();

        public async Task<HttpResponseMessage> SendRender(ReplaySettings settings, MemoryStream replayDataStream)
        {
            using MultipartFormDataContent data = new();
            using StreamContent streamContent = new(replayDataStream);

            int skinId = settings.Skin is null ? DEFAULT_SKIN : settings.Skin.Id;

            if (DefaultVerificationKey != null)
                settings.VerificationKey = DefaultVerificationKey;
            
            data.Add(streamContent, name: "replayFile");
            data.Add(new StringContent(BOT_NAME), name: "username");
            data.Add(new StringContent(DEFAULT_RESOLUTION), name: "resolution");
            data.Add(new StringContent(skinId.ToString()), name: "skin");

            Type type = settings.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                string requestName = property.Name.FirstCharToLower();
                string? requestValue = property.GetValue(settings)?.ToString();
                if (requestValue is not null)
                    data.Add(new StringContent(requestValue), name: requestName);
            }
        }
    }
}
