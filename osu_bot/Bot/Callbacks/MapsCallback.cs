// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Telegram.Bot;
using Telegram.Bot.Types;

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
            if (update.CallbackQuery?.Message == null)
            {
                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.Message.Chat,
                text: text,
                cancellationToken: cancellationToken);
        }
    }
}
