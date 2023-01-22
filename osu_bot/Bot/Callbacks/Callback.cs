using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace osu_bot.Bot.Callbacks
{
    public abstract class Callback
    {
        public abstract string Data { get; }

        public abstract Task Action(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}
