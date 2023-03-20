// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API;
using osu_bot.Entites.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Commands
{
    public class RegCommand : ICommand
    {
        public string CommandText => "/reg";

        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuAPI _api = OsuAPI.Instance;

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

            string text = message.Text;
            int startIndex = text.IndexOf(' ') + 1;
            string name = text[startIndex..].ToLower();

            TelegramUser telegramUser = _database.TelegramUsers.FindOne(u => u.OsuName.ToLower() == name);
            if (telegramUser != null)
            {
                throw new ArgumentException($"Аккаунт {telegramUser.OsuName} уже привязан к другому пользователю");
            }

            Entites.OsuUser osuUser = await _api.GetUserInfoByUsernameAsync(name);

            telegramUser = _database.TelegramUsers.FindById(message.From.Id);
            if (telegramUser != null)
            {
                telegramUser.OsuName = osuUser.Name;
                telegramUser.OsuId = osuUser.Id;
                telegramUser.ChatId = message.Chat.Id;
                _database.TelegramUsers.Update(telegramUser);
            }
            else
            {
                _database.TelegramUsers.Insert(new TelegramUser(message.From.Id, osuUser.Id, osuUser.Name, message.Chat.Id));
            }

            await botClient.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"Аккаунт {osuUser.Name} успешно привязан к Вашему аккаунту",
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
        }
    }
}
