using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using osu_bot.API;

namespace osu_bot.Bot.Commands
{
    public abstract class Command
    {
        public OsuAPI API { get; set; }

        public abstract string Text { get; }

        public abstract Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}
