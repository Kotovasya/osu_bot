// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;
using osu_bot.Entites.Database;

namespace osu_bot.Entites
{
    public class OsuBeatmapAttributes
    {
        [BsonId]
        [JsonIgnore]
        public BeatmapAttributesKey Id { get; set; }

        [JsonIgnore]
        public long BeatmapId { get; set; }

        [JsonIgnore]
        public int Mods { get; set; }

        [JsonProperty("star_rating")]
        public float Stars { get; set; }

        [JsonProperty("aim_difficulty")]
        public double AimDifficulty { get; set; }

        [JsonProperty("flashlight_difficulty")]
        public double FlashlightDifficulty { get; set; }

        [JsonProperty("slider_factor")]
        public double SliderFactor { get; set; }

        [JsonProperty("speed_note_count")]
        public double SpeedNoteCount { get; set; }

        [JsonProperty("speed_difficulty")]
        public double SpeedDifficulty { get; set; }

        public double CS { get; set; }
        public double AR { get; set; }
        public double OD { get; set; }
        public double HP { get; set; }

        public int Length { get; set; }
        public double BPM { get; set; }
    }
}
