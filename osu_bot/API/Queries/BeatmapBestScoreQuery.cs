// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API.Parameters;
using osu_bot.Entites;
using osu_bot.Exceptions;

namespace osu_bot.API.Queries
{
    public class BeatmapBestScoreQuery : Query<BeatmapBestScoresQueryParameters, OsuScoreInfo?>
    {
        protected override async Task<OsuScoreInfo?> RunAsync()
        {
            ArgumentNullException.ThrowIfNull(Parameters.Username);
            OsuUser userInfo = await API.GetUserInfoByUsernameAsync(Parameters.Username);
            if (userInfo.Id == 0)
            {
                throw new ArgumentException($"Пользователь с именем {Parameters.Username} не найден");
            }

            Parameters.UserId = userInfo.Id;

            Newtonsoft.Json.Linq.JToken jsonScore = await API.GetJsonAsync(UrlParameter);

            OsuScoreInfo score = new()
            {
                User = userInfo
            };

            if (jsonScore["error"] != null)
            {
                return null;
            }
            else
            {
                score.ParseScoreJson(jsonScore["score"]);
            }

            return score;
        }
    }
}
