using Newtonsoft.Json.Linq;
using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Queries
{
    public class BeatmapAttributesQuery : Query<BeatmapAttribute>
    {
        public BeatmapAttributesQuery() 
        {
            UrlParameter = "https://osu.ppy.sh/api/v2/beatmaps/%beatmapId/attributes";
        }

        public long BeatmapId { get; set; }
        public int Mods { get; set; }

        public override async Task<BeatmapAttribute> ExecuteAsync(OsuAPI api)
        {
            var jObject = JObject.FromObject(new
            {
                mods = Mods,
                ruleset = "osu"
            });
            BeatmapAttribute beatmapAttributes = new();
            var response = await api.PostJsonAsync(UrlParameter.Replace("%beatmapId", BeatmapId.ToString()), jObject);
            beatmapAttributes.MaxCombo = (int)response["attributes"]["max_combo"];
            beatmapAttributes.Stars = (double)response["attributes"]["star_rating"];
            return beatmapAttributes;
        }
    }
}
/*
 * {
    "attributes": {
        "star_rating": 4.956999778747559,
        "max_combo": 447,
        "aim_difficulty": 2.5800299644470215,
        "speed_difficulty": 2.1105499267578125,
        "speed_note_count": 120.0469970703125,
        "flashlight_difficulty": 0,
        "slider_factor": 0.9993990063667297,
        "approach_rate": 9,
        "overall_difficulty": 8.5
    }
}
 */