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
    public class OsuBeatmap
    {
        [BsonId]
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("difficulty_rating")]
        public float Stars { get; set; }

        [JsonProperty("ranked")]
        public OsuBeatmapStatus Status { get; set; }

        [JsonProperty("is_scoreable")]
        public bool IsScoreable { get; set; }

        [JsonProperty("total_length")]
        public int TotalLength { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("version")]
        public string DifficultyName { get; set; }

        [JsonProperty("accuracy")]
        public float OD { get; set; }

        [JsonProperty("ar")]
        public float AR { get; set; }

        [JsonProperty("cs")]
        public float CS { get; set; }

        [JsonProperty("drain")]
        public float HP { get; set; }

        [JsonProperty("bpm")]
        public float BPM { get; set; }

        [JsonProperty("count_circles")]
        public int CountCircles { get; set; }

        [JsonProperty("count_sliders")]
        public int CountSliders { get; set; }

        [JsonProperty("drain")]
        public int CountSpinners { get; set; }

        [JsonProperty("hit_length")]
        public int HitLength { get; set; }

        [JsonProperty("last_updated")]
        public DateTime LastUpdated { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("beatmapset_id")]
        public long BeatmapsetId { get; set; }

        [BsonRef]
        [JsonProperty("beatmapset")]
        public OsuBeatmapset Beatmapset { get; set; }


        public int TotalObjects => CountCircles + CountSliders + CountSpinners;
    }
}
