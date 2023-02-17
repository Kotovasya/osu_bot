using osu_bot.Entites.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Commands.Main
{
    public class RegCommand : Command
    {
        public override string Text => "/reg";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text == null)
                return;

            var message = update.Message;
            string text = message.Text;
            string name = text.Substring(text.IndexOf(' '), text.Length - 1);

            if (Database.TelegramUsers.Exists(u => u.OsuName == name))
                throw new ArgumentException($"Пользователь с именем {name} уже зарегистрирован");

            var osuUser = await API.GetUserInfoByUsernameAsync(name);

            var telegramUser = Database.TelegramUsers.FindById(message.From.Id);
            if (telegramUser != null)
            {
                telegramUser.OsuName = name;
                telegramUser.OsuId = osuUser.Id;
            }
            else
                Database.TelegramUsers.Insert(new TelegramUser(message.From.Id, osuUser.Id, osuUser.Name));
        }
    }
}
