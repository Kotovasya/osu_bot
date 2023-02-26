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
        private static readonly string connectionString = @"Database.db";

        private static readonly DatabaseContext instance = new(connectionString);
        
        public static DatabaseContext Instance { get => instance; }

        private readonly LiteDatabase db;

        private DatabaseContext(string connectionString)
        {
            db = new LiteDatabase(connectionString);
        }

        public ILiteCollection<TelegramUser> TelegramUsers => db.GetCollection<TelegramUser>();
    }
}
