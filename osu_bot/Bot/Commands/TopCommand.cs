// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Bot.Callbacks;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Commands
{
    //top <number> <username> <+MODS>
    public class TopCommand : ICommand
    {
        private readonly UserScoresQuery _userScoresQuery = new();

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public string CommandText => "/top";

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

            UserScoreQueryParameters parameters = _userScoresQuery.Parameters;

            Message message = update.Message;
            string args = message.Text.Trim()[CommandText.Length..];
            parameters.Parse(args);

            if (parameters.Username == null || message.Text == CommandText)
            {
                TelegramUser telegramUser = _database.TelegramUsers.FindOne(u => u.Id == message.From.Id);
                parameters.Username = telegramUser != null
                    ? telegramUser.OsuName
                    : throw new Exception("Аккаунт Osu не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");
            }

            List<ScoreInfo> scores = await _userScoresQuery.ExecuteAsync();
            if (scores.Count == 0)
            {
                if (parameters.Mods == null)
                {
                    throw new Exception($"У пользователя {parameters.Username} отсутствуют топ скоры");
                }
                else
                {
                    throw new Exception
                        ($"У пользователя {parameters.Username} отсутствуют топ скоры с {ModsConverter.ToString(parameters.Mods)}");
                }
            }

            SKImage image;
            string? caption = null;
            InlineKeyboardMarkup? inlineKeyboard = null;
            if (scores.Count > 1)
            {
                image = await ImageGenerator.Instance.CreateScoresCardAsync(scores);
            }
            else
            {
                ScoreInfo score = scores.First();
                image = await ImageGenerator.Instance.CreateFullCardAsync(score);
                caption = score.Beatmap.Url;
                inlineKeyboard = new(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "🎯Мой скор", callbackData: $"{MyScoreCallback.DATA} beatmapId{score.Beatmap.Id})"),
                        InlineKeyboardButton.WithCallbackData(text: "🏆Топ конфы", callbackData: $"{TopConferenceCallback.DATA} beatmapId{score.Beatmap.Id})")
                    });
            }

            _ = await botClient.SendPhotoAsync(
                chatId: update.Message.Chat,
                caption: caption,
                photo: new InputOnlineFile(image.Encode().AsStream()),
                replyToMessageId: update.Message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
