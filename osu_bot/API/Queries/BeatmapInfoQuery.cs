// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API.Parameters;
using osu_bot.Entites;

namespace osu_bot.API.Queries
{
    public class BeatmapInfoQuery : Query<BeatmapInfoQueryParameters, Beatmap>
    {
        protected override async Task<Beatmap> RunAsync()
        {
            Newtonsoft.Json.Linq.JToken queryResult = await API.GetJsonAsync(UrlParameter);
            Beatmap result = new();
            result.ParseBeatmapJson(queryResult);
            result.ParseBeatmapsetJson(queryResult["beatmapset"]);

            return result;
        }
    }
}
