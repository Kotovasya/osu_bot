// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;
using osu_bot.Modules.Converters;

namespace osu_bot.Entites
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class OsuScore
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("accuracy")]
        public float Accuracy { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("passed")]
        public bool IsPassed { get; set; }

        [JsonProperty("pp")]
        public float? PP { get; set; }

        [JsonProperty("rank")]
        public string Rank { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("statistics.count_300")]
        public int Count300 { get; set; }

        [JsonProperty("statistics.count_100")]
        public int Count100 { get; set; }

        [JsonProperty("statistics.count_50")]
        public int Count50 { get; set; }

        [JsonProperty("statistics.count_miss")]
        public int CountMisses { get; set; }

        [JsonProperty("mods")]
        public int Mods { get; set; }

        [BsonRef]
        public OsuBeatmap Beatmap { get; set; }

        [BsonRef]
        public OsuUser User { get; set; }

        [BsonRef]
        public OsuBeatmapAttributes BeatmapAttributes { get; set; }


        public int HitObjects => Count300 + Count100 + Count50 + CountMisses;
    }
}
