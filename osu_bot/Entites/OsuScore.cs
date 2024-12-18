﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using osu_bot.Modules.Converters;

namespace osu_bot.Entites
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class OsuScore
    {
        private float _accuracy;
        private OsuBeatmapset _beatmapset;

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("accuracy")]
        public float Accuracy
        {
            get => _accuracy;
            set
            {
                if (value <= 1)
                    value *= 100;
                _accuracy = value;
            }
        }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("passed")]
        public bool IsPassed { get; set; }

        [JsonProperty("replay")]
        public bool IsReplay { get; set; }

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

        [JsonConverter(typeof(JsonModsConverter))]
        [JsonProperty("mods")]
        public int Mods { get; set; }

        [JsonProperty("beatmap")]
        [BsonRef]
        public OsuBeatmap Beatmap { get; set; }

        [JsonProperty("beatmapset")]
        [BsonRef]
        public OsuBeatmapset Beatmapset
        {
            get => _beatmapset;
            set
            {
                _beatmapset = value;
                Beatmap.Beatmapset = value;
            }
        }

        [JsonProperty("user")]
        [BsonRef]
        public OsuUser User { get; set; }

        [BsonRef]
        public OsuBeatmapAttributes BeatmapAttributes { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public int HitObjects => Count300 + Count100 + Count50 + CountMisses;

        public bool IsFullCombo => MaxCombo / BeatmapAttributes.MaxCombo >= 0.99;

        public string CalculateRank()
        {
            string resultRank;
            float procent300 = Count300 / HitObjects;
            float procent50 = Count50 / HitObjects;

            if (!IsPassed)
                resultRank = "F";
            else if (Accuracy == 100)
                resultRank = "X";
            else if (procent300 > 0.9 && procent50 < 0.01 && CountMisses == 0)
                resultRank = "S";
            else if ((procent300 > 0.8 && CountMisses == 0) || (procent300 > 0.9))
                resultRank = "A";
            else if ((procent300 > 0.7 && CountMisses == 0) || (procent300 > 0.8))
                resultRank = "B";
            else if (procent300 > 0.6)
                resultRank = "C";
            else
                resultRank = "D";

            if (resultRank is "X" or "S")
            {
                IEnumerable<Mod> mods = ModsConverter.ToMods(Mods);
                if (mods.Any(m => m.Number is ModHidden.NUMBER or ModFlashlight.NUMBER))
                    resultRank += "H";
            }
            Rank = resultRank;
            return resultRank;    
        }

        public float CalculateAccuracy()
        {
            Accuracy = (float)PerfomanceCalculator.CalculateAccuracyFromHits(Count300, Count100, Count50, CountMisses);
            return Accuracy;
        }
    }
}
