// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API.Parameters;
using osu_bot.Entites;

namespace osu_bot.API.Queries
{
    public class BeatmapBestScoreQuery : Query<BeatmapBestScoresQueryParameters, ScoreInfo>
    {

        private readonly BeatmapInfoQuery _beatmapInfoQuery = new();
        private readonly BeatmapAttributesJsonQuery _beatmapAttributesJsonQuery = new();

        protected override async Task<ScoreInfo> RunAsync()
        {
            ArgumentNullException.ThrowIfNull(Parameters.Username);
            User userInfo = await API.GetUserInfoByUsernameAsync(Parameters.Username);
            if (userInfo.Id == 0)
            {
                throw new ArgumentException($"Пользователь с именем {Parameters.Username} не найден");
            }

            Parameters.UserId = userInfo.Id;

            Newtonsoft.Json.Linq.JToken jsonScore = await API.GetJsonAsync(UrlParameter);
            if (jsonScore["error"] != null)
            {
                throw new Exception($"У пользователя {Parameters.Username} отсутствуют скоры на карте {Parameters.BeatmapId}");
            }

            ScoreInfo score = new();
            score.ParseScoreJson(jsonScore["score"]);
            score.User = userInfo;

            _beatmapInfoQuery.Parameters.BeatmapId = Parameters.BeatmapId;
            score.Beatmap = await _beatmapInfoQuery.ExecuteAsync();

            _beatmapAttributesJsonQuery.Parameters.BeatmapId = Parameters.BeatmapId;
            _beatmapAttributesJsonQuery.Parameters.Mods = score.Mods;
            score.Beatmap.Attributes.ParseDifficultyAttributesJson(await _beatmapAttributesJsonQuery.ExecuteAsync());
            score.Beatmap.Attributes.CalculateAttributesWithMods(score.Mods);

            return score;
        }
    }
}
