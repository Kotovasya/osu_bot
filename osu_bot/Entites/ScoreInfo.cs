using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace osu_bot.Entites
{
    public class ScoreInfo
    {
        public ScoreInfo()
        {
            Beatmap = new Beatmap();
            User = new User();
        }

        public void ParseScoreJson(JToken json)
        {
            if (json == null)
                return;

            if (json["id"] != null)
                Id = json["id"].Value<long>();

            if (json["score"] != null)
                Score = json["score"].Value<int>();

            if (json["accuracy"] != null)
                Accuracy = json["accuracy"].Value<float>() * 100;

            if (json["created_at"] != null)
            {
                string value = json.SelectToken("created_at").Value<string>();
                Date = DateTime.ParseExact(value, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToLocalTime();
            }

            if (json["max_combo"] != null)
                MaxCombo = json["max_combo"].Value<int>();

            if (json["pp"] != null)
                PP = json["pp"].Value<float>();

            if (json["statistics"] != null)
                ParseScoreStatisticsJson(json["statistics"]);

            if (json["rank"] != null)
                Rank = json["rank"].Value<string>();

            if (json["mods"] != null)
                Mods = ModsParser.ConvertToMods(string.Concat(json["mods"].Values<string>()));

            if (json["beatmap"] != null)
                Beatmap.ParseBeatmapJson(json["beatmap"]);

            if (json["beatmapset"] != null)
                Beatmap.ParseBeatmapsetJson(json["beatmapset"]);

            if (json["user"] != null)
                User.ParseUserJson(json["user"]);
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
