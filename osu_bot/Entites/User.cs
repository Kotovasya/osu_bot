using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace osu_bot.Entites
{
    public class User
    {
        public User()
        {

        }

        public void ParseUserJson(JsonObject json)
        {
            if (json == null)
                return;

            if (json.TryGetPropertyValue("id", out JsonNode? node))
                Id = node.GetValue<int>();

            if (json.TryGetPropertyValue("username", out node))
                Name = node.GetValue<string>();

            if (json.TryGetPropertyValue("statistics", out node))
                ParseUserStatisticsJson(node.AsObject());

            if (json.TryGetPropertyValue("country_code", out node))
                CountryCode = node.GetValue<string>();

            if (json.TryGetPropertyValue("avatar_url", out node))
                AvatarUrl = node.GetValue<string>();

            if (json.TryGetPropertyValue("join_date", out node))
                DateRegistration = DateTime.Parse(node.GetValue<string>());

            if (json.TryGetPropertyValue("last_visit", out node))
                LastOnline = DateTime.Parse(node.GetValue<string>());

            if (json.TryGetPropertyValue("rank_history", out node))
                RankHistory = node["data"].AsArray().Select(s => s.GetValue<int>()).ToArray();           
        }

        public void ParseUserStatisticsJson(JsonObject? json)
        {
            if (json == null)
                return;

            if (json.TryGetPropertyValue("pp", out JsonNode? node))
                PP = node.GetValue<int>();

            if (json.TryGetPropertyValue("rank", out node))
            {
                WorldRating = node["global"].GetValue<int>();
                CountryRating = node["country"].GetValue<int>();
            }

            if (json.TryGetPropertyValue("play_time", out node))
                PlayTime = node.GetValue<int>();

            if (json.TryGetPropertyValue("play_count", out node))
                PlayCount = node.GetValue<int>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int PP { get; set; }
        public int WorldRating { get; set; }
        public int CountryRating { get; set; }
        public float Accuracy { get; set; }
        public int PlayTime { get; set; }
        public int PlayCount { get; set; }
        public string CountryCode { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime DateRegistration { get; set; }
        public DateTime LastOnline { get; set; }

        public int[] RankHistory { get; set; }
    }
}
