using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Parameters
{
    public class BeatmapBestScoresQueryParamters : IQueryParameters
    {
        public int UserId { get; set; }
        public string Username { get; set; }

        public int BeatmapId { get; set; }
        public string GetQueryString()
        {
            return $"https://osu.ppy.sh/api/v2/beatmaps/{BeatmapId}/scores/users/{UserId}/";
        }
    }
}
