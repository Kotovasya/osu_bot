using osu_bot.API.Parameters;
using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Queries
{
    public class BeatmapScoresQuery : IQuery<List<ScoreInfo>>
    {
        public BeatmapScoresQueryParameters Parameters { get; set; }

        public string UrlParameter => Parameters.GetQueryString();

        public async Task<List<ScoreInfo>> ExecuteAsync(OsuAPI api)
        {
            var userInfo = await api.GetUserInfoByUsernameAsync(Parameters.Username);
            if (userInfo.Id == 0)
                throw new ArgumentException($"Пользователь с именем {Parameters.Username} не найден");
            Parameters.UserId = userInfo.Id;

            var result = new List<ScoreInfo>();
            var queryResult = await api.GetJsonAsync(UrlParameter);
            if (queryResult["error"] != null)
                return result;

            foreach(var jsonScore in queryResult["scores"])
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
