using osu_bot.API.Parameters;
using osu_bot.Entites;
using osu_bot.Entites.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.API.Queries
{
    public class BeatmapBestScoreQuery : IQuery<ScoreInfo>
    {
        public BeatmapBestScoresQueryParamters Parameters = new();

        public string UrlParameter => Parameters.GetQueryString();

        public async Task<ScoreInfo> ExecuteAsync(OsuAPI api)
        {
            var userInfo = await api.GetUserInfoByUsernameAsync(Parameters.Username);
            if (userInfo.Id == 0)
                throw new ArgumentException($"Пользователь с именем {Parameters.Username} не найден");
            Parameters.UserId = userInfo.Id;

            var jsonScore = await api.GetJsonAsync(UrlParameter);
            if (jsonScore["error"] != null)
                throw new Exception($"У пользователя {Parameters.Username} отсутствуют скоры на карте {Parameters.BeatmapId}");

            var score = new ScoreInfo();
            score.ParseScoreJson(jsonScore["score"]);
            score.User = userInfo;

            var beatmapQuery = new BeatmapInfoQuery(Parameters.BeatmapId);
            score.Beatmap = await beatmapQuery.ExecuteAsync(api);

            var beatmapAttributesQuery = new BeatmapAttributesQuery();
            beatmapAttributesQuery.Parameters.BeatmapId = Parameters.BeatmapId;
            beatmapAttributesQuery.Parameters.Mods = score.Mods;
            score.Beatmap.Attributes.ParseDifficultyAttributesJson(await beatmapAttributesQuery.GetJson(api));
            score.Beatmap.Attributes.CalculateAttributesWithMods(score.Mods);

            return score;
        }
    }
}
