using osu_bot.API.Parameters;
using osu_bot.Entites;
using osu_bot.Entites.Mods;
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

            List<ScoreInfo> resultScores = new();

            var jsonScores = await api.GetJsonArrayAsync(UrlParameter);

            foreach (var jsonScore in jsonScores)
            {
                ScoreInfo score = new();
                score.ParseScoreJson(jsonScore);
                resultScores.Add(score);
            }
            resultScores = resultScores
                .Where(s => Parameters.Mods == null ||
                    new HashSet<Mod>(s.Mods).SetEquals(Parameters.Mods))
                .ToList();

            return resultScores;
        }
    }
}
