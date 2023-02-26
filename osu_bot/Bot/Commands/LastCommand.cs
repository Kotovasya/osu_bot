using osu_bot.API;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Bot.Callbacks;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using osu_bot.Modules;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Commands
{
    internal class LastCommand : Command
    {
        private readonly UserScoresQuery userScoresQuery = new();

        private readonly DatabaseContext Database = DatabaseContext.Instance;

        public override string CommandText => "/last";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text == null)
                return;

            var parameters = userScoresQuery.Parameters;
            parameters.IsRecent = true;

            var message = update.Message;
            var args = message.Text.Trim()[CommandText.Length..];
            parameters.Parse(args);

            if (parameters.Username == null || message.Text == CommandText)
            {
                var telegramUser = Database.TelegramUsers.FindOne(u => u.Id == message.From.Id);
                if (telegramUser != null)
                    parameters.Username = telegramUser.OsuName;
                else
                    throw new Exception("Аккаунт Osu не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");
            }

            List<ScoreInfo> scores = await userScoresQuery.ExecuteAsync();
            if (scores.Count == 0)
            {
                if (parameters.Mods == null)
                    throw new Exception($"У пользователя {parameters.Username} отсутствуют скоры за последние 24 часа");
                else
                    throw new Exception
                        ($"У пользователя {parameters.Username} отсутствуют скоры {ModsConverter.ToString(parameters.Mods)} за последние 24 часа");
            }

            byte[]? imageStream;
            string? caption = null;
            InlineKeyboardMarkup? inlineKeyboard = null;
            if (scores.Count > 1)
                imageStream = ImageGenerator.CreateScoresCard(scores).ToStream();
            else
            {
                var score = scores.First();
                imageStream = ImageGenerator.CreateFullCard(score).ToStream();
                caption = score.Beatmap.Url;
                inlineKeyboard = new(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "🎯Мой скор", callbackData: $"{MyScoreCallback.DATA} beatmapId{score.Beatmap.Id})"),
                        InlineKeyboardButton.WithCallbackData(text: "🏆Топ конфы", callbackData: $"{TopConferenceCallback.DATA} beatmapId{score.Beatmap.Id})")
                    });
            }

            await botClient.SendPhotoAsync(
                chatId: update.Message.Chat,
                caption: caption,
                photo: new InputOnlineFile(new MemoryStream(imageStream)),
                replyToMessageId: update.Message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
