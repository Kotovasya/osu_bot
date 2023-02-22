using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace osu_bot.Bot.Callbacks
{
    public class MapsCallback : Callback
    {
        private static readonly string text =
@"В сообщении с командами ниже ОБЯЗАНА быть ссылка на карту (либо в пересланном сообщении)
/best <pp|score|combo|acc> <username> <+MODS>
Найти свой, или указанного игрока лучший скор на карте
Пример:
/best peppy - Показать лучший (по очкам) скор на карте у игрока peppy

/conf <+MODS>
Показать лучшие скоры игроков в чате
";

        public const string DATA = "Maps callback";

        public override string Data => DATA;

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.Message.Chat,
                text: text,
                cancellationToken: cancellationToken);
        }
    }
}
