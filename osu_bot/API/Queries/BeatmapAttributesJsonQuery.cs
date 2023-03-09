// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Newtonsoft.Json.Linq;
using osu_bot.API.Parameters;

namespace osu_bot.API.Queries
{
    public class BeatmapAttributesJsonQuery : Query<BeatmapAttributesQueryParameters, JToken>
    {
        protected override async Task<JToken> RunAsync()
        {
            JObject json = Parameters.GetJson();
            return await API.PostJsonAsync(UrlParameter, json);
        }
    }
}
