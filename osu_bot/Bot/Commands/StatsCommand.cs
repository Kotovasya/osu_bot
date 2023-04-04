// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Exceptions;
using osu_bot.Modules;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;

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
                return;

            if (message.From == null)
                return;

            string text = message.Text.Trim();
            string name;
            if (text == CommandText)
            {
                TelegramUser telegramUser = _database.TelegramUsers
                    .Include(u => u.OsuUser)
                    .FindOne(u => u.Id == message.From.Id);
                if (telegramUser != null)
                    name = telegramUser.OsuUser.Username;
                else
                    throw new UserNotRegisteredException();
            }
            else
            {
                int startIndex = text.IndexOf(' ') + 1;
                name = text[startIndex..];
            }

            OsuUser? user = await _service.GetUserAsync(name);

            if (user is null || user.Id == 0)
                throw new UserNotFoundException(name);

            using SKImage image = await ImageGenerator.Instance.CreateProfileCardAsync(user);

            await botClient.SendPhotoAsync(
                chatId: message.Chat,
                photo: new InputFile(image.Encode().AsStream()),
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
    }
}
