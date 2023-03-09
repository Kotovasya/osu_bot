﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace osu_bot.Bot.Commands
{
    public class StatsCommand : ICommand
    {
        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuAPI _api = OsuAPI.Instance;

        public string CommandText => "/stats";

        public async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text == null)
            {
                return;
            }

            if (update.Message.From == null)
            {
                return;
            }

            Message message = update.Message;
            string text = message.Text.Trim();
            string name;
            if (text == CommandText)
            {
                TelegramUser telegramUser = _database.TelegramUsers.FindOne(u => u.Id == message.From.Id);
                name = telegramUser != null
                    ? telegramUser.OsuName
                    : throw new Exception("Аккаунт Osu! не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");
            }
            else
            {
                int startIndex = text.IndexOf(' ') + 1;
                name = text.Length > startIndex
                    ? text[startIndex..]
                    : throw new Exception("Неверно указано имя пользователя Osu! в команде, синтаксис /stats <username>");
            }

            Entites.User userInfo = await _api.GetUserInfoByUsernameAsync(name);
            if (userInfo.Id == 0)
            {
                throw new ArgumentException($"Пользователь с именем {name} не найден");
            }

            SKImage image = await ImageGenerator.Instance.CreateProfileCardAsync(userInfo);

            _ = await botClient.SendPhotoAsync(
                chatId: update.Message.Chat,
                photo: new InputOnlineFile(image.Encode().AsStream()),
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken);
        }
    }
}