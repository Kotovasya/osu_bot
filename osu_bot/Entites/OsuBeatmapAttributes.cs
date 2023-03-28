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
using osu_bot.Entites.Mods;
using osu_bot.Modules.Converters;

namespace osu_bot.Entites
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class OsuBeatmapAttributes
    {
        [BsonId]
        [JsonIgnore]
        public BeatmapAttributesKey Id { get; set; }

        [JsonProperty("attributes.max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("attributes.star_rating")]
        public float Stars { get; set; }

        [JsonProperty("attributes.aim_difficulty")]
        public double AimDifficulty { get; set; }

        [JsonProperty("attributes.flashlight_difficulty")]
        public double FlashlightDifficulty { get; set; }

        [JsonProperty("attributes.slider_factor")]
        public double SliderFactor { get; set; }

        [JsonProperty("attributes.speed_note_count")]
        public double SpeedNoteCount { get; set; }

        [JsonProperty("attributes.speed_difficulty")]
        public double SpeedDifficulty { get; set; }

        [JsonIgnore]
        public double CS { get; set; }
        [JsonIgnore]
        public double AR { get; set; }
        [JsonIgnore]
        public double OD { get; set; }
        [JsonIgnore]
        public double HP { get; set; }
        [JsonIgnore]
        public int TotalLength { get; set; }
        [JsonIgnore]
        public int HitLength { get; set; }
        [JsonIgnore]
        public double BPM { get; set; }

        public void CopyBeatmapAttributes(OsuBeatmap beatmap)
        {
            CS = beatmap.CS;
            AR = beatmap.AR;
            OD = beatmap.OD;
            HP = beatmap.HP;
            TotalLength = beatmap.TotalLength;
            HitLength = beatmap.HitLength;
            BPM = beatmap.BPM;
        }

        public void CalculateAttributesWithMods(IEnumerable<Mod> mods)
        {
            if (!mods.Any())
                return;

            var applicableMods = mods.Where(m => m is IApplicableMod).Select(m => m as IApplicableMod);
            var firstApplicableMods = applicableMods.Where(m => m is ModHardRock || m is ModEasy);

            foreach (var mod in firstApplicableMods)
                mod.ApplyToAttributes(this);

            applicableMods = applicableMods.Except(firstApplicableMods);
            foreach (var mod in applicableMods)
                mod.ApplyToAttributes(this);
        }
    }
}
