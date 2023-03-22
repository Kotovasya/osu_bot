// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace osu_bot.Entites.Database
{
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
        public float PP { get; set; }

        [JsonProperty("rank")]
        public string Rank { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("statistics.count_300")]
        public int Count300 { get; set; }

        [JsonProperty("count_100")]
        public int Count100 { get; set; }

        [JsonProperty("count_50")]
        public int Count50 { get; set; }

        [JsonProperty("count_miss")]
        public int CountMises { get; set; }

        [JsonProperty("mods")]
        public int Mods { get; set; }

        [JsonProperty("max_combo")]


        [JsonProperty("max_combo")]


        [JsonProperty("max_combo")]


        [JsonProperty("max_combo")]
    }
}
