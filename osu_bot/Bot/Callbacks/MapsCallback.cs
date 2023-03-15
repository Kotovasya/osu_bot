// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Callbacks
{
    public class MapsCallback : ICallback
    {
        private static readonly string _text =
@"В сообщении с командами ниже ОБЯЗАНА быть ссылка на карту (либо в пересланном сообщении)
/best <pp|score|combo|acc> <username> <+MODS>
Найти свой, или указанного игрока лучший скор на карте
Пример:
/best peppy - Показать лучший (по очкам) скор на карте у игрока peppy

/conf <+MODS>
Показать лучшие скоры игроков в чате
";

        public const string DATA = "Maps callback";

        public string Data => DATA;

        public async Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message == null)
            {
                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat,
                text: _text,
                cancellationToken: cancellationToken);
        }
    }
}
