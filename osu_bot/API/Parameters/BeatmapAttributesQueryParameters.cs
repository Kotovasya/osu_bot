using Newtonsoft.Json.Linq;
using osu_bot.Entites;
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
        public Mods Mods { get; set; }

        public string GetQueryString()
        {
            return $"https://osu.ppy.sh/api/v2/beatmaps/{BeatmapId}/attributes";
        }

        public JObject GetJson()
        {
            return JObject.FromObject(new
            {
                mods = ModsParser.ConvertToInt(Mods),
                ruleset = "osu"
            });
        }
    }
}
