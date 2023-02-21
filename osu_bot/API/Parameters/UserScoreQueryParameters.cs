using osu_bot.Entites;
using osu_bot.Entites.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace osu_bot.API.Parameters
{
    public abstract class UserScoreQueryParameters 
        : IQueryParameters
    {
        public string? Username { get; set; }

        public int UserId { get; set; }

        public IEnumerable<Mod>? Mods { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

        public abstract string GetQueryString();
    }

    public class UserTopScoreQueryParameters : UserScoreQueryParameters
    {
        public UserTopScoreQueryParameters()
        {
            Limit = 5;
        }

        public override string GetQueryString()
        {
            return $"https://osu.ppy.sh/api/v2/users/{UserId}/scores/best?limit=100&offset={Offset}";
        }
    }

    public class UserLastScoreQueryParameters : UserScoreQueryParameters
    {
        public bool IncludeFails { get; set; }

        public UserLastScoreQueryParameters()
        {
            Limit = 1;
            IncludeFails = true;
        }

        public override string GetQueryString()
        {
            return $"https://osu.ppy.sh/api/v2/users/{UserId}/scores/recent?include_fails={(IncludeFails ? 1 : 0)}&limit=100&offset={Offset}";
        }
    }
}
