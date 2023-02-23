using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Queries
{
    public class BeatmapInfoQuery : IQuery<Beatmap>
    {
        public int BeatmapId { get; set; }
        public string UrlParameter => $"https://osu.ppy.sh/api/v2/beatmaps/{BeatmapId}";

        public BeatmapInfoQuery(int beatmapId)
        {
            BeatmapId = beatmapId;
        }

        public async Task<Beatmap> ExecuteAsync(OsuAPI api)
        {
            var queryResult = await api.GetJsonAsync(UrlParameter);
            var result = new Beatmap();
            result.ParseBeatmapJson(queryResult);
            result.ParseBeatmapsetJson(queryResult["beatmapset"]);

            return result;
        }
    }
}
