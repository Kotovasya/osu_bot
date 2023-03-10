// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API.Parameters;
using osu_bot.Entites;

namespace osu_bot.API.Queries
{
    public class BeatmapInfoQuery : Query<BeatmapInfoQueryParameters, OsuBeatmap>
    {
        protected override async Task<OsuBeatmap> RunAsync()
        {
            Newtonsoft.Json.Linq.JToken queryResult = await API.GetJsonAsync(UrlParameter);
            OsuBeatmap result = new();
            result.ParseBeatmapJson(queryResult);
            result.ParseBeatmapsetJson(queryResult["beatmapset"]);

            return result;
        }
    }
}
