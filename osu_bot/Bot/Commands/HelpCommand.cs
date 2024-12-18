﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Bot.Callbacks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Commands
{
    public class HelpCommand : ICommand
    {
        public string CommandText => "/help";

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "⚙️Основные", callbackData: HelpCallback.DATA),
                    InlineKeyboardButton.WithCallbackData(text: "🗺Карты", callbackData: MapsCallback.DATA)
                });

            await botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: "Информация о каких командах тебя интересует?",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
