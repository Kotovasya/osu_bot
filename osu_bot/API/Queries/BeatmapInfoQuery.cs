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
    public class BeatmapAttributesQuery : IQuery<BeatmapAttributes>
    {
        public BeatmapAttributesQueryParameters Parameters = new();
        public string UrlParameter => Parameters.GetQueryString();

        public async Task<BeatmapAttributes> ExecuteAsync(OsuAPI api)
        {      
            var response = await api.PostJsonAsync(UrlParameter, Parameters.GetJson());
            BeatmapAttributes beatmapAttributes = new();
            beatmapAttributes.ParseBeatmapAttributesJson(response, Parameters.Mods);
            return beatmapAttributes;
        }

        public async Task<JToken> GetJson(OsuAPI api)
        {
            var json = Parameters.GetJson();
            return await api.PostJsonAsync(UrlParameter, json);
        }
    }
}