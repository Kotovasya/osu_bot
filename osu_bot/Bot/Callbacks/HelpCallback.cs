using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Callbacks
{
    public class HelpCallback : Callback
    {
        private static readonly string text =
@"/reg [username]
Привязать аккаунт osu! к аккаунту Telegram

/stats <username>
Вывести статистику аккаунта

/top <number> <username> <+MODS>
Показать свой(и), или указанного игрока топ скор(ы)
Примеры:
/top peppy - показать топ-5 скоров игрока peppy
/top 24 peppy - показать #24 скор игрока peppy
/top peppy +HD - показать топ-5 скоров игрока peppy с модом HD

/last <number> <username> <+MODS> <+pass>
Показать свой/указанного игрока последний скор
Примеры:
/last peppy - показать последний скор игрока peppy
/last 5 peppy +HD - показать последние 5 скоров игрока peppy с модом HD
/last 3 +HD +pass - показать свои последние пасснутые 3 скора с модом HD
";

        public const string DATA = "Help callback";

        public override string Data => DATA;

        public override async Task Action(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.Message.Chat,
                text: text,
                cancellationToken: cancellationToken);
        }
    }
}
