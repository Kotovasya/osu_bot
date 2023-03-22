// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace osu_bot.API.Parameters
{
    public class BeatmapBestScoresQueryParameters : IQueryParameters
    {
        public long UserId { get; set; }
        public string? Username { get; set; }

        public long BeatmapId { get; set; }
        public string GetQueryString() => $"https://osu.ppy.sh/api/v2/beatmaps/{BeatmapId}/scores/users/{UserId}/";
    }
}
