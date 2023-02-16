using Newtonsoft.Json.Linq;
using osu_bot.Entites;
using osu_bot.Entites.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Parameters
{
    public class BeatmapAttributesQueryParameters : IQueryParameters, IJsonParameters
    {
        public long BeatmapId { get; set; }
        public IEnumerable<Mod>? Mods { get; set; }

        public string GetQueryString()
        {
            return $"https://osu.ppy.sh/api/v2/beatmaps/{BeatmapId}/attributes";
        }

        public JObject GetJson()
        {
            return JObject.FromObject(new
            {
                mods = Mods?.Select(m => m.Name),
                ruleset = "osu"
            });
        }
    }
}
