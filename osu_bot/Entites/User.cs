using System;
using System.Collections.Generic;
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

        public User(JsonObject json)
        {

        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int PP { get; set; }
        public int WorldRating { get; set; }
        public int CountryRating { get; set; }
        public double Accuracy { get; set; }
        public string PlayTime { get; set; }
        public int PlayCount { get; set; }
        public string CountryCode { get; set; }
        public string AvatarUrl { get; set; }
        public string DateRegistration { get; set; }
        public string LastOnline { get; set; }

        public int[] RankHistroy { get; set; }
    }
}
