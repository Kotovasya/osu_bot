using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Database
{
    public class TelegramUser
    {
        public TelegramUser(long id, int osuId, string osuName)
        {
            Id = id;
            OsuId = osuId;
            OsuName = osuName;
        }

        public long Id { get; set; }

        public int OsuId { get; set; }

        public string OsuName { get; set; }
    }
}
