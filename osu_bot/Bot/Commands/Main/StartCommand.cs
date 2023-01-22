using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Commands.Main
{
    public class StartCommand : Command
    {
        public override string Text => "/start";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat,
                text: "Кто разбудил древнее зло? Напишите /help идиоты, вдруг это вам поможет",
                cancellationToken: cancellationToken);
            return;
        }
    }
}
