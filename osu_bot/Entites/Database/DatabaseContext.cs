using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Database
{
    public class DatabaseContext
    {
        public DatabaseContext(LiteDatabase database)
        {
            TelegramUsers = database.GetCollection<TelegramUser>();
        }

        public ILiteCollection<TelegramUser> TelegramUsers { get; private set; }
    }
}
