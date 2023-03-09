// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API.Parameters;
using osu_bot.Entites;

namespace osu_bot.API.Queries
{
    public class BeatmapScoresQuery : Query<BeatmapScoresQueryParameters, List<ScoreInfo>>
    {
        protected override async Task<List<ScoreInfo>> RunAsync()
        {
            ArgumentNullException.ThrowIfNull(Parameters.Username);
            User userInfo = await API.GetUserInfoByUsernameAsync(Parameters.Username);
            if (userInfo.Id == 0)
            {
                throw new ArgumentException($"Пользователь с именем {Parameters.Username} не найден");
            }

            Parameters.UserId = userInfo.Id;

            List<ScoreInfo> result = new();
            Newtonsoft.Json.Linq.JToken queryResult = await API.GetJsonAsync(UrlParameter);
            if (queryResult["error"] != null)
            {
                return result;
            }

            foreach (Newtonsoft.Json.Linq.JToken? jsonScore in queryResult["scores"])
            {
                ScoreInfo score = new();
                score.ParseScoreJson(jsonScore);
                score.User = userInfo;
                result.Add(score);
            }

            return result;
        }
    }
}
