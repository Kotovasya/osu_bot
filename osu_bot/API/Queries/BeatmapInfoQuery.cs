using Newtonsoft.Json.Linq;
using osu_bot.API.Parameters;
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
        public BeatmapAttributesQueryParameters Parameters = new();
        public override string UrlParameter => Parameters.GetQueryString();

        public override async Task<BeatmapAttributes> ExecuteAsync(OsuAPI api)
        {      
            var response = await api.PostJsonAsync(UrlParameter, Parameters.GetJson());
            BeatmapAttributes beatmapAttributes = new();
            beatmapAttributes.ParseBeatmapAttributes(response, Parameters.Mods);
            return beatmapAttributes;
        }

        public async Task<JToken> GetJson(OsuAPI api)
        {
            var json = Parameters.GetJson();
            return await api.PostJsonAsync(UrlParameter, json);
        }
    }
}