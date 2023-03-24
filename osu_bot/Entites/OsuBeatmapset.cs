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
    public class OsuBeatmapset
    {
        [BsonId]
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("cover.cover@2x")]
        public string CoverUrl { get; set; }

        [JsonProperty("creator")]
        public string Mapper { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("play_count")]
        public int PlayCount { get; set; }

        [JsonProperty("is_scoreable")]
        public bool IsScoreable { get; set; }

        [JsonProperty("preview_url")]
        public string AudioUrl { get; set; }

        [JsonProperty("ranked")]
        public OsuBeatmapStatus Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("user_id")]
        public long MapperId { get; set; }
    }
}
