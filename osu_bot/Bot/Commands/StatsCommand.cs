// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API;
using osu_bot.Entites;
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
        private readonly OsuService _service = OsuService.Instance;

        public string CommandText => "/stats";

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text == null)
            {
                return;
            }

            if (message.From == null)
            {
                return;
            }

            string text = message.Text.Trim();
            string name;
            if (text == CommandText)
            {
                TelegramUser telegramUser = _database.TelegramUsers.FindOne(u => u.Id == message.From.Id);
                name = telegramUser != null
                    ? telegramUser.OsuUser.Username
                    : throw new Exception("Аккаунт Osu! не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");
            }
            else
            {
                int startIndex = text.IndexOf(' ') + 1;
                name = text.Length > startIndex
                    ? text[startIndex..]
                    : throw new Exception("Неверно указано имя пользователя Osu! в команде, синтаксис /stats <username>");
            }

            OsuUser? user = await _service.GetUserAsync(name);

            if (user is null)
                throw new NotImplementedException();

            if (user.Id == 0)
                throw new ArgumentException($"Пользователь с именем {name} не найден");

            SKImage image = await ImageGenerator.Instance.CreateProfileCardAsync(user);

            await botClient.SendPhotoAsync(
                chatId: message.Chat,
                photo: new InputOnlineFile(image.Encode().AsStream()),
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
    }
}
