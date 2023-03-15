// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Newtonsoft.Json.Linq;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules;

namespace osu_bot.Entites
{
    public class OsuScoreInfo
    {
#pragma warning disable CS8618
        public OsuScoreInfo()
        {
            Beatmap = new OsuBeatmap();
            User = new OsuUser();
        }

        public OsuScoreInfo(ScoreInfo score)
        {
            Beatmap = new OsuBeatmap();
            User = new OsuUser();
            Id = score.Id;
            Score = score.Score;
            Accuracy = score.Accuracy;
            Date = score.Date;
            MaxCombo = score.MaxCombo;
            Count50 = score.Count50;
            Count100 = score.Count100;
            Count300 = score.Count300;
            CountMisses = score.CountMisses;
            Rank = score.Rank;
            Beatmap.Id = score.BeatmapId;
            Mods = ModsConverter.ToMods(score.Mods);
        }
#pragma warning restore CS8618

        public void ParseScoreJson(JToken json)
        {
            if (json == null)
            {
                return;
            }

            if (json["id"] != null)
            {
                Id = json["id"].Value<long>();
            }

            if (json["score"] != null)
            {
                Score = json["score"].Value<int>();
            }

            if (json["accuracy"] != null)
            {
                Accuracy = json["accuracy"].Value<float>() * 100;
            }

            if (json["created_at"] != null)
            {
                string value = json.SelectToken("created_at").Value<string>();
                Date = DateTime.ParseExact(value, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToLocalTime();
            }

            if (json["max_combo"] != null)
            {
                MaxCombo = json["max_combo"].Value<int>();
            }

            if (json["pp"] != null)
            {
                PP = json["pp"].Value<float?>();
            }

            if (json["statistics"] != null)
            {
                ParseScoreStatisticsJson(json["statistics"]);
            }

            if (json["rank"] != null)
            {
                Rank = json["rank"].Value<string>();
            }

            if (json["mods"] != null)
            {
                Mods = ModsConverter.ToMods(json["mods"].Values<string>());
            }

            if (json["beatmap"] != null)
            {
                Beatmap.ParseBeatmapJson(json["beatmap"]);
            }

            if (json["beatmapset"] != null)
            {
                Beatmap.ParseBeatmapsetJson(json["beatmapset"]);
            }

            if (json["user"] != null)
            {
                User.ParseUserJson(json["user"]);
            }
        }

        private void ParseScoreStatisticsJson(JToken json)
        {
            Count50 = json["count_50"].Value<int>();
            Count100 = json["count_100"].Value<int>();
            Count300 = json["count_300"].Value<int>();
            CountMisses = json["count_miss"].Value<int>();
        }

        public long Id { get; set; }
        public int Score { get; set; }
        public float Accuracy { get; set; }
        public DateTime Date { get; set; }
        public int MaxCombo { get; set; }
        public float? PP { get; set; }
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public int Count300 { get; set; }
        public int CountMisses { get; set; }
        public string Rank { get; set; }
        public IEnumerable<Mod> Mods { get; set; }
        public OsuBeatmap Beatmap { get; set; }
        public OsuUser User { get; set; }

        public int HitObjects => Count300 + Count100 + Count50 + CountMisses;

        public bool IsFullCombo => MaxCombo / Beatmap.Attributes.TotalObjects >= 0.99;

        public float Compilation => HitObjects / Beatmap.Attributes.TotalObjects;
    }
}
