// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Newtonsoft.Json.Linq;
using osu_bot.Entites.Mods;

namespace osu_bot.API.Parameters
{
    public class BeatmapAttributesQueryParameters : IQueryParameters, IJsonParameters
    {
        public long BeatmapId { get; set; }
        public IEnumerable<Mod>? Mods { get; set; }

        public string GetQueryString() => $"https://osu.ppy.sh/api/v2/beatmaps/{BeatmapId}/attributes";

        public JObject GetJson()
        {
            IEnumerable<string> queryMods = Mods == null || Mods.Any(m => m.Name == "NM") ? (IEnumerable<string>)Array.Empty<string>() : Mods.Select(m => m.Name);
            return JObject.FromObject(new
            {
                mods = queryMods,
                ruleset = "osu"
            });
        }
    }
}
