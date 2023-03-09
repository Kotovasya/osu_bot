// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace osu_bot.API.Parameters
{
    public class BeatmapInfoQueryParameters : IQueryParameters
    {
        public int BeatmapId { get; set; }

        public string GetQueryString() => $"https://osu.ppy.sh/api/v2/beatmaps/{BeatmapId}";
    }
}
