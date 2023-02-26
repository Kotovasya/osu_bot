using osu_bot.API;
using osu_bot.Entites.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Commands
{
    public class RegCommand : Command
    {
        public override string CommandText => "/reg";

        private readonly DatabaseContext Database = DatabaseContext.Instance;
        private readonly OsuAPI API = OsuAPI.Instance;

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text == null)
                return;

            var message = update.Message;
            string text = message.Text;
            int startIndex = text.IndexOf(' ') + 1;
            string name = text[startIndex..].ToLower();

            var users = Database.TelegramUsers.FindAll().ToArray();

            if (Database.TelegramUsers.Exists(u => u.OsuName == name))
                throw new ArgumentException($"Аккаунт {name} уже привязан к другому пользователю");

            var osuUser = await API.GetUserInfoByUsernameAsync(name);

            var telegramUser = Database.TelegramUsers.FindById(message.From.Id);
            if (telegramUser != null)
            {
                telegramUser.OsuName = name;
                telegramUser.OsuId = osuUser.Id;
                Database.TelegramUsers.Update(telegramUser);
            }
            else
                Database.TelegramUsers.Insert(new TelegramUser(message.From.Id, osuUser.Id, osuUser.Name));

            await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: $"Аккаунт {name} успешно привязан к Вашему аккаунту",
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);
        }
    }
}
