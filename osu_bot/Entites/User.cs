using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PP { get; set; }
        public int WorldRating { get; set; }
        public int CountryRating { get; set; }
        public string PlayTime { get; set; }
        public int PlayCount { get; set; }

        public string AvatarUrl { get; set; }
    }
}
