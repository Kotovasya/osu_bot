using osu_bot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace osu_bot.Bot.Commands.Main
{
    public class StatsCommand : Command
    {
        public override string Text => "/stats";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text == null)
                return;

            var message = update.Message;
            string text = message.Text.Trim();
            string name;
            if (text == Text)
            {
                var telegramUser = Database.TelegramUsers.FindOne(u => u.Id == message.From.Id);
                if (telegramUser != null)
                    name = telegramUser.OsuName;
                else
                    throw new Exception("Аккаунт Osu! не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");
            }
            else
            {
                int startIndex = text.IndexOf(' ') + 1;
                if (text.Length > startIndex)
                    name = text[startIndex..];
                else
                    throw new Exception("Неверно указано имя пользователя Osu! в команде, синтаксис /stats <username>");
            }

            var userInfo = await API.GetUserInfoByUsernameAsync(name);
            if (userInfo.Id == 0)
                throw new ArgumentException($"Пользователь с именем {name} не найден");

            var image = ImageGenerator.CreateProfileCard(userInfo);
            var imageStream = image.ToStream();
            
            await botClient.SendPhotoAsync(
                chatId: update.Message.Chat,
                photo: new InputOnlineFile(new MemoryStream(imageStream)),
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken);
        }
    }
}
