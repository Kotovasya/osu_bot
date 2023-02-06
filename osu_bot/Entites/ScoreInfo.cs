using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace osu_bot.Entites
{
    public class ScoreInfo
    {
        public ScoreInfo()
        {
            Beatmap = new Beatmap();
            User = new User();
        }

        public void ParseScoreJson(JsonObject json)
        {
            if (json == null)
                return;

            if (json.TryGetPropertyValue("id", out JsonNode? node))
                Id = node.GetValue<long>();

            if (json.TryGetPropertyValue("score", out node))
                Score = node.GetValue<int>();

            if (json.TryGetPropertyValue("accuracy", out node))
                Accuracy = node.GetValue<float>();

            if (json.TryGetPropertyValue("created_at", out node))
                Date = DateTime.Parse(node.GetValue<string>());

            if (json.TryGetPropertyValue("max_combo", out node))
                MaxCombo = node.GetValue<int>();

            if (json.TryGetPropertyValue("pp", out node))
                PP = node.GetValue<float>();

            if (json.TryGetPropertyValue("statistics", out node))
                ParseScoreStatisticsJson(node.AsObject());

            if (json.TryGetPropertyValue("rank", out node))
                Rank = node.GetValue<string>();

            if (json.TryGetPropertyValue("mods", out node))
            {
                Mods mods = ModsParser.ConvertToMods(string.Concat(node.GetValue<string[]>));
                Mods = mods;
                Beatmap.Attributes.Mods = mods;
            }

            if (json.TryGetPropertyValue("beatmap", out node))
                Beatmap.ParseBeatmapJson(node.AsObject());

            if (json.TryGetPropertyValue("beatmapset", out node))
                Beatmap.ParseBeatmapsetJson(node.AsObject());

            if (json.TryGetPropertyValue("user", out node))
                User.ParseUserJson(node.AsObject());
        }

        private void ParseScoreStatisticsJson(JsonObject json)
        {
            if (json.TryGetPropertyValue("count_50", out JsonNode? node))
                Count50 = node.GetValue<int>();

            if (json.TryGetPropertyValue("count_100", out node))
                Count100 = node.GetValue<int>();

            if (json.TryGetPropertyValue("count_300", out node))
                Count300 = node.GetValue<int>();

            if (json.TryGetPropertyValue("count_miss", out node))
                CountMisses = node.GetValue<int>();
        }

        public long Id { get; set; }
        public int Score { get; set; }
        public float Accuracy { get; set; }
        public DateTime Date { get; set; }
        public int MaxCombo { get; set; }
        public float PP { get; set; }
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public int Count300 { get; set; }
        public int CountMisses { get; set; }
        public string Rank { get; set; }
        public Mods Mods { get; set; }
        public Beatmap Beatmap { get; set; }
        public User User { get; set; }
    }
}
