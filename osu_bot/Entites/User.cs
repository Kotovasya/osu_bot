using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
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

        public void ParseUserJson(JToken json)
        {
            if (json == null)
                return;

            if (json["id"] != null)
                Id = json["id"].Value<int>();
            
            if (json["username"] != null)
                Name = json["username"].Value<string>();

            if (json["statistics"] != null)
                ParseUserStatisticsJson(json["statistics"]);

            if (json["country_code"] != null)
                CountryCode = json["country_code"].Value<string>();

            if (json["avatar_url"] != null)
                AvatarUrl = json["avatar_url"].Value<string>();

            if (json["join_date"] != null)
            {
                string value = json.SelectToken("join_date").Value<string>();
                DateRegistration = DateTime.ParseExact(value, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToLocalTime();
            };           

            if (json["last_visit"] != null)
            {
                string value = json.SelectToken("last_visit").Value<string>();
                if (value != null)
                    LastOnline = DateTime.ParseExact(value, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToLocalTime();
            }

            if (json["rank_history"] != null)
                RankHistory = json["rank_history"]["data"].Values<int>().ToArray();           
        }

        private void ParseUserStatisticsJson(JToken? json)
        {
            if (json == null)
                return;

            PP = json["pp"].Value<int>();
            WorldRating = json["global_rank"].Value<int>();
            CountryRating = json["country_rank"].Value<int>();
            PlayTime = TimeSpan.FromSeconds(json["play_time"].Value<int>());
            PlayCount = json["play_count"].Value<int>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int PP { get; set; }
        public int WorldRating { get; set; }
        public int CountryRating { get; set; }
        public float Accuracy { get; set; }
        public TimeSpan PlayTime { get; set; }
        public int PlayCount { get; set; }
        public string CountryCode { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime DateRegistration { get; set; }
        public DateTime? LastOnline { get; set; }

        public int[] RankHistory { get; set; }
    }
}
