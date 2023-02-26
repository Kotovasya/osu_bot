using osu_bot.API.Parameters;
using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Queries
{
    public class BeatmapInfoQuery : Query<BeatmapInfoQueryParameters, Beatmap>
    {
        protected override async Task<Beatmap> RunAsync()
        {
            var queryResult = await API.GetJsonAsync(UrlParameter);
            var result = new Beatmap();
            result.ParseBeatmapJson(queryResult);
            result.ParseBeatmapsetJson(queryResult["beatmapset"]);

            return result;
        }
    }
}
