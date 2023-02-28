using osu_bot.API.Parameters;
using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Queries
{
    public class BeatmapScoresQuery : Query<BeatmapScoresQueryParameters, List<ScoreInfo>>
    {
        protected override async Task<List<ScoreInfo>> RunAsync()
        {
            var userInfo = await API.GetUserInfoByUsernameAsync(Parameters.Username);
            if (userInfo.Id == 0)
                throw new ArgumentException($"Пользователь с именем {Parameters.Username} не найден");
            Parameters.UserId = userInfo.Id;

            var result = new List<ScoreInfo>();
            var queryResult = await API.GetJsonAsync(UrlParameter);
            if (queryResult["error"] != null)
                return result;

            foreach (var jsonScore in queryResult["scores"])
            {
                var score = new ScoreInfo();
                score.ParseScoreJson(jsonScore);
                score.User = userInfo;
                result.Add(score);
            }

            return result;
        }
    }
}
