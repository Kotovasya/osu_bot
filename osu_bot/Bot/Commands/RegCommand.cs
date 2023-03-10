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
            string text = message.Text;
            int startIndex = text.IndexOf(' ') + 1;
            string name = text[startIndex..].ToLower();

            TelegramUser[] users = _database.TelegramUsers.FindAll().ToArray();

            if (_database.TelegramUsers.Exists(u => u.OsuName == name))
            {
                throw new ArgumentException($"Аккаунт {name} уже привязан к другому пользователю");
            }

            Entites.OsuUser osuUser = await _api.GetUserInfoByUsernameAsync(name);

            TelegramUser telegramUser = _database.TelegramUsers.FindById(message.From.Id);
            if (telegramUser != null)
            {
                telegramUser.OsuName = name;
                telegramUser.OsuId = osuUser.Id;
                _database.TelegramUsers.Update(telegramUser);
            }
            else
            {
                _database.TelegramUsers.Insert(new TelegramUser(message.From.Id, osuUser.Id, osuUser.Name));
            }

            await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: $"Аккаунт {name} успешно привязан к Вашему аккаунту",
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);
        }
    }
}
