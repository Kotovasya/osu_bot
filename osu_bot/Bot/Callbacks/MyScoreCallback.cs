// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Callbacks
{
    public class MyScoreCallback : Callback
    {
        public const string DATA = "My score";
        public override string Data => DATA;

        private readonly BeatmapBestScoreQuery _beatmapBestScoreQuery = new();

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery?.Data == null)
            {
                return;
            }

            if (update.CallbackQuery.Message == null)
            {
                return;
            }

            BeatmapBestScoresQueryParameters parameters = _beatmapBestScoreQuery.Parameters;

            TelegramUser telegramUser = _database.TelegramUsers.FindOne(u => u.Id == update.CallbackQuery.From.Id);
            parameters.Username = telegramUser != null
                ? telegramUser.OsuName
                : throw new Exception("Аккаунт Osu не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");

            string data = update.CallbackQuery.Data;

            Match beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);
            if (!beatmapIdMatch.Success)
            {
                throw new Exception("При обработке запроса \"Мой скор\" произошла ошибка считывания ID карты");
            }

            parameters.BeatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);

            ScoreInfo score = await _beatmapBestScoreQuery.ExecuteAsync();

            SKImage image = await ImageGenerator.Instance.CreateFullCardAsync(score);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "🎯Мой скор", callbackData: $"{DATA} beatmapId{score.Beatmap.Id})"),
                    InlineKeyboardButton.WithCallbackData(text: "🏆Топ конфы", callbackData: MapsCallback.DATA)
                });


            _ = await botClient.SendPhotoAsync(
                chatId: update.CallbackQuery.Message.Chat,
                photo: new InputOnlineFile(image.EncodedData.AsStream()),
                replyToMessageId: update.CallbackQuery.Message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
