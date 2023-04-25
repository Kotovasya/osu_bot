// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Modules;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Commands
{
    public class ReplaySettingsCommand : ICommand
    {
        public string CommandText => "/rs";

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text is null)
                return;

            if (message.From is null)
                return;

            if (message.Chat.Id != message.From.Id)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Настроить пресеты для реплеев можно только в ЛС бота",
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Используй кнопки ниже для создания или редактирования пресетов",
                    replyMarkup: MarkupGenerator.Instance.ReplaySettingsList(message.From.Id, 1),
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
