using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace osu_bot.API.Parameters
{
    public abstract class ScoreQueryParameters : IQueryParameters
    {
        public int UserId { get; set; }

        public Mods Mods { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

        public abstract string GetQueryString();
    }

    public class TopScoreQueryParameters : ScoreQueryParameters
    {
        public override string GetQueryString()
        {
            return $"https://osu.ppy.sh/api/v2/users/{UserId}/scores/best?limit=100&offset={Offset}";
        }
    }

    public class LastScoreQueryParameters : ScoreQueryParameters
    {
        public bool IncludeFails { get; set; }

        public override string GetQueryString()
        {
            return $"https://osu.ppy.sh/api/v2/users/{UserId}/scores/recent?include_fails={(IncludeFails ? 1 : 0)}&limit=100&offset={Offset}";
        }
    }
}
