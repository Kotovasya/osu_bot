// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using osu_bot.API.Parameters;
using osu_bot.Entites;

namespace osu_bot.API.Queries
{
    public class ScoreQuery : Query<ScoreQueryParameters, OsuScoreInfo>
    {

        private readonly BeatmapAttributesJsonQuery _beatmapAttributesJsonQuery = new();

        protected override async Task<OsuScoreInfo> RunAsync()
        {
            OsuUser userInfo = await API.GetUserInfoByUsernameAsync(Parameters.Username);

            JToken jsonScore = await API.GetJsonAsync(UrlParameter);
            OsuScoreInfo score = new();
            score.ParseScoreJson(jsonScore);
            score.User = userInfo;

            _beatmapAttributesJsonQuery.Parameters.Mods = score.Mods;
            _beatmapAttributesJsonQuery.Parameters.BeatmapId = score.Beatmap.Id;
            score.User = userInfo;
            score.Beatmap.Attributes.ParseDifficultyAttributesJson(await _beatmapAttributesJsonQuery.ExecuteAsync());
            score.Beatmap.Attributes.CalculateAttributesWithMods(score.Mods);

            return score;
        }
    }
}
