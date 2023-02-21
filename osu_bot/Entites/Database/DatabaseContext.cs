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
        private readonly LiteDatabase db;

        public DatabaseContext(string connectionString)
        {
            db = new LiteDatabase(connectionString);
        }

        public ILiteCollection<TelegramUser> TelegramUsers => db.GetCollection<TelegramUser>();
    }
}
