// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Commands
{
    public class RegCommand : ICommand
    {
        public string CommandText => "/reg";

        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text is null)
                return;

            if (message.From is null)
                return; 

            string text = message.Text;
            int startIndex = text.IndexOf(' ') + 1;
            string name = text[startIndex..].ToLower();

            TelegramUser telegramUser = _database.TelegramUsers.FindOne(u => u.OsuUser.Username.ToLower() == name);
            if (telegramUser != null)
            {
                throw new ArgumentException($"Аккаунт {telegramUser.OsuUser.Username} уже привязан к другому пользователю");
            }

            OsuUser? osuUser = await _service.GetUserAsync(name);

            if (osuUser is null)
                throw new NotImplementedException();

            TelegramUser newTelegramUser = new(message.From.Id, message.Chat.Id, osuUser);
            _database.TelegramUsers.Upsert(newTelegramUser);

            await botClient.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"Аккаунт {osuUser.Username} успешно привязан к Вашему аккаунту",
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
        }
    }
}
