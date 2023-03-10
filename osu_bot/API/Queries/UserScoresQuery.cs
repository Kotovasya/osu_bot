// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Newtonsoft.Json.Linq;
using osu_bot.API.Parameters;
using osu_bot.Entites;
using osu_bot.Entites.Mods;

namespace osu_bot.API.Queries
{
    public class UserScoresQuery : Query<UserScoreQueryParameters, List<OsuScoreInfo>>
    {
        private readonly BeatmapAttributesJsonQuery _beatmapAttributesJsonQuery = new();

        protected override async Task<List<OsuScoreInfo>> RunAsync()
        {
            ArgumentNullException.ThrowIfNull(Parameters.Username);
            OsuUser userInfo = await API.GetUserInfoByUsernameAsync(Parameters.Username);
            if (userInfo.Id == 0)
            {
                throw new ArgumentException($"Пользователь с именем {Parameters.Username} не найден");
            }

            Parameters.UserId = userInfo.Id;

            List<OsuScoreInfo> resultScores = new();

            JArray jsonScores = await API.GetJsonArrayAsync(UrlParameter);

            foreach (JToken? jsonScore in jsonScores)
            {
                OsuScoreInfo score = new();
                score.ParseScoreJson(jsonScore);
                resultScores.Add(score);
            }

            resultScores = resultScores
                .Where(s => Parameters.Mods == null ||
                    new HashSet<Mod>(s.Mods).SetEquals(Parameters.Mods))
                .Skip(Parameters.Offset)
                .Take(Parameters.Limit)
                .ToList();

            foreach (OsuScoreInfo score in resultScores)
            {
                _beatmapAttributesJsonQuery.Parameters.Mods = score.Mods;
                _beatmapAttributesJsonQuery.Parameters.BeatmapId = score.Beatmap.Id;
                score.User = userInfo;
                score.Beatmap.Attributes.ParseDifficultyAttributesJson(await _beatmapAttributesJsonQuery.ExecuteAsync());
                score.Beatmap.Attributes.CalculateAttributesWithMods(score.Mods);
            }
            return resultScores;
        }
    }
}
