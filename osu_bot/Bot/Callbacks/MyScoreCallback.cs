// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Exceptions;
using osu_bot.Modules;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Callbacks
{
    public class MyScoreCallback : ICallback
    {
        public const string DATA = "My score";
        public string Data => DATA;

        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;

        public async Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return;

            if (callbackQuery.Message == null)
                return;

            TelegramUser telegramUser = _database.TelegramUsers
                .Include(u => u.OsuUser)
                .FindOne(u => u.Id == callbackQuery.From.Id);

            if (telegramUser == null)
                throw new UserNotRegisteredException();

            string data = callbackQuery.Data;

            Match beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);
            if (!beatmapIdMatch.Success)
            {
                throw new Exception("При обработке запроса \"Мой скор\" произошла ошибка считывания ID карты");
            }

            long beatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);
            long userId = telegramUser.OsuUser.Id;

            OsuScore? score = await _service.GetUserBeatmapBestScoreAsync(beatmapId, userId);

            if (score is null)
                throw new UserScoresNotFound(telegramUser.OsuUser.Username, beatmapId);

            SKImage image = await ImageGenerator.Instance.CreateFullCardAsync(score);

            InlineKeyboardMarkup inlineKeyboard = Extensions.ScoreKeyboardMarkup(score.Beatmap.Id, score.Beatmapset.Id);

            await botClient.SendPhotoAsync(
                chatId: callbackQuery.Message.Chat,
                photo: new InputOnlineFile(image.Encode().AsStream()),
                replyToMessageId: callbackQuery.Message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
