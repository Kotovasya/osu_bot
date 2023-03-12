// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Newtonsoft.Json.Linq;
using osu_bot.Entites.Mods;

namespace osu_bot.Entites
{
    public class OsuBeatmap
    {
#pragma warning disable CS8618
        public OsuBeatmap() => Attributes = new OsuBeatmapAttributes();
#pragma warning restore CS8618

        public void ParseBeatmapJson(JToken json)
        {
            if (json != null)
            {
                if (json["id"] != null)
                {
                    Id = json["id"].Value<long>();
                }

                if (json["version"] != null)
                {
                    DifficultyName = json["version"].Value<string>();
                }

                if (json["url"] != null)
                {
                    Url = json["url"].Value<string>();
                }

                Attributes.ParseBeatmapAttributesJson(json);
            }
        }

        public void ParseBeatmapsetJson(JToken json)
        {
            if (json != null)
            {
                if (json["title"] != null)
                {
                    Title = json["title"].Value<string>();
                }

                if (json["covers"] != null)
                {
                    CoverUrl = json["covers"]["cover@2x"].Value<string>();
                }

                if (json["status"] != null)
                {
                    Status = json["status"].Value<string>();
                }

                if (json["artist"] != null)
                {
                    Artist = json["artist"].Value<string>();
                }

                if (json["creator"] != null)
                {
                    MapperName = json["creator"].Value<string>();
                }
            }
        }

        public long Id { get; set; }
        public string Title { get; set; }
        public string DifficultyName { get; set; }
        public string CoverUrl { get; set; }
        public string Artist { get; set; }
        public string Url { get; set; }
        public string MapperName { get; set; }
        public string Status { get; set; }
        public OsuBeatmapAttributes Attributes { get; set; }

        public bool ScoresTable => Status != "graveyard" && Status != "wip" && Status != "pending";
    }
}
