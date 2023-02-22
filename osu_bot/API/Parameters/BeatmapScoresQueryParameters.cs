using osu_bot.Entites.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Parameters
{
    public class BeatmapScoresQueryParameters : IQueryParameters
    {
        public int BeatmapId { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; }

        public IEnumerable<Mod>? Mods { get; set; }

        public bool IsAll { get; set; } 

        public string GetQueryString()
        {
            return $"https://osu.ppy.sh/api/v2/beatmaps/{BeatmapId}/scores/users/{UserId}/{(IsAll ? "all" : string.Empty)}";
        }
    }
}
