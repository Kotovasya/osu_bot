// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;
using osu_bot.Modules.Converters;

namespace osu_bot.Entites
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class OsuUser
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [AllowNull]
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("country_code")]
        public string? CountryCode { get; set; }

        [JsonProperty("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("is_supporter")]
        public bool IsSupport { get; set; }

        [JsonProperty("is_online")]
        public bool IsOnline { get; set; }

        [JsonProperty("last_visit")]
        public DateTime? LastVisit { get; set; }

        [JsonProperty("join_date")]
        public DateTime JoinDate { get; set; }

        [AllowNull]
        [JsonConverter(typeof(JsonPlaycountConverter))]
        [JsonProperty("monthly_playcounts")]
        public IList<int> MonthlyPlaycounts { get; set; }

        #region Statistics
        [JsonProperty("statistics.level.current")]
        public int Level { get; set; }

        [JsonProperty("statistics.level.progress")]
        public int LevelProgress { get; set; }

        [JsonProperty("statistics.grade_counts.ss")]
        public int SSGradeCount { get; set; }

        [JsonProperty("statistics.grade_counts.ssh")]
        public int SSHGradeCount { get; set; }

        [JsonProperty("statistics.grade_counts.s")]
        public int SGradeCount { get; set; }

        [JsonProperty("statistics.grade_counts.sh")]
        public int SHGradeCount { get; set; }

        [JsonProperty("statistics.grade_counts.a")]
        public int AGradeCount { get; set; }

        [JsonProperty("statistics.pp")]
        public int PP { get; set; }

        [JsonProperty("statistics.hit_accuracy")]
        public float HitAccuracy { get; set; }

        [JsonProperty("statistics.maximum_combo")]
        public int MaximumCombo { get; set; }

        [JsonProperty("rank_highest.rank")]
        public int HighestRank { get; set; }

        [JsonProperty("rank_highest.updated_at")]
        public DateTime HighestRankDate { get; set; }

        [JsonProperty("statistics.global_rank")]
        public int GlobalRank { get; set; }

        [JsonProperty("statistics.country_rank")]
        public int CountryRank { get; set; }

        [JsonProperty("statistics.play_count")]
        public int PlayCount { get; set; }

        [JsonProperty("statistics.play_time")]
        public int PlayTime { get; set; }

        [JsonProperty("statistics.ranked_score")]
        public long RankedScore { get; set; }

        [JsonProperty("statistics.total_score")]
        public long TotalScore { get; set; }

        [JsonProperty("statistics.total_hits")]
        public int TotalHits { get; set; }
        #endregion

        [JsonProperty("support_level")]
        public int SupportLevel { get; set; }

        [AllowNull]
        [JsonProperty("rank_history.data")]
        public IList<int> RankHistory { get; set; }
    }
}
