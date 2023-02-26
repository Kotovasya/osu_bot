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
    public class BeatmapAttributesJsonQuery : Query<BeatmapAttributesQueryParameters, JToken>
    {
        protected override async Task<JToken> RunAsync()
        {
            var json = Parameters.GetJson();
            return await API.PostJsonAsync(UrlParameter, json);
        }
    }
}