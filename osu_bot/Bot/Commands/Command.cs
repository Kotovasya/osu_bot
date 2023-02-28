using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using osu_bot.API;
using LiteDB;
using osu_bot.Entites.Database;

namespace osu_bot.Bot.Commands
{
    public abstract class Command
    {
        public abstract string CommandText { get; }

        public abstract Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}
