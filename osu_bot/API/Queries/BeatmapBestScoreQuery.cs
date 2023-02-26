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
    public class BeatmapBestScoreQuery : Query<BeatmapBestScoresQueryParameters, ScoreInfo>
    {

        private readonly BeatmapInfoQuery beatmapInfoQuery = new();
        private readonly BeatmapAttributesJsonQuery beatmapAttributesJsonQuery = new();

        protected override async Task<ScoreInfo> RunAsync()
        {
            var userInfo = await API.GetUserInfoByUsernameAsync(Parameters.Username);
            if (userInfo.Id == 0)
                throw new ArgumentException($"Пользователь с именем {Parameters.Username} не найден");
            Parameters.UserId = userInfo.Id;

            var jsonScore = await API.GetJsonAsync(UrlParameter);
            if (jsonScore["error"] != null)
                throw new Exception($"У пользователя {Parameters.Username} отсутствуют скоры на карте {Parameters.BeatmapId}");

            var score = new ScoreInfo();
            score.ParseScoreJson(jsonScore["score"]);
            score.User = userInfo;

            beatmapInfoQuery.Parameters.BeatmapId = Parameters.BeatmapId;
            score.Beatmap = await beatmapInfoQuery.ExecuteAsync();

            beatmapAttributesJsonQuery.Parameters.BeatmapId = Parameters.BeatmapId;
            beatmapAttributesJsonQuery.Parameters.Mods = score.Mods;
            score.Beatmap.Attributes.ParseDifficultyAttributesJson(await beatmapAttributesJsonQuery.ExecuteAsync());
            score.Beatmap.Attributes.CalculateAttributesWithMods(score.Mods);

            return score;
        }
    }
}
