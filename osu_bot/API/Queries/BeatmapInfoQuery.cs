using Newtonsoft.Json.Linq;
using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Queries
{
    public class BeatmapAttributesQuery : Query<BeatmapAttributes>
    {
        public BeatmapAttributesQuery() 
        {
            UrlParameter = "https://osu.ppy.sh/api/v2/beatmaps/%beatmapId/attributes";
        }

        public long BeatmapId { get; set; }
        public int Mods { get; set; }

        public override async Task<BeatmapAttributes> ExecuteAsync(OsuAPI api)
        {
            var jObject = JObject.FromObject(new
            {
                mods = Mods,
                ruleset = "osu"
            });
            
            var response = await api.PostJsonAsync(UrlParameter.Replace("%beatmapId", BeatmapId.ToString()), jObject);
            BeatmapAttributes beatmapAttributes = new();
            beatmapAttributes.ParseBeatmapAttributes(response);
            return beatmapAttributes;
        }

        public async Task<JToken> GetJson(OsuAPI api)
        {
            var jObject = JObject.FromObject(new
            {
                mods = Mods,
                ruleset = "osu"
            });

            return await api.PostJsonAsync(UrlParameter.Replace("%beatmapId", BeatmapId.ToString()), jObject);
        }
    }
}