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
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace osu_bot.Bot.Callbacks
{
    public class MyScoreCallback : ICallback
    {
        public const string DATA = "My score";
        public string Data => DATA;

        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;

        public async Task<CallbackResult?> ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return null;

            if (callbackQuery.Message == null)
                return null;

            TelegramUser telegramUser = _database.TelegramUsers
                .Include(u => u.OsuUser)
                .FindOne(u => u.Id == callbackQuery.From.Id);

            if (telegramUser == null)
                return new CallbackResult(new UserNotRegisteredException().Message, 500);

            string data = callbackQuery.Data;

            Match beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);
            if (!beatmapIdMatch.Success)
                return new CallbackResult("При обработке запроса произошла ошибка считывания ID карты");
            

            long beatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);
            long userId = telegramUser.OsuUser.Id;

            OsuScore? score = await _service.GetUserBeatmapBestScoreAsync(beatmapId, userId);

            if (score is null)
                return new CallbackResult(new UserScoresNotFound(telegramUser.OsuUser.Username, beatmapId).Message, 500);

            SKImage image = await ImageGenerator.Instance.CreateFullCardAsync(score);

            await botClient.SendPhotoAsync(
                chatId: callbackQuery.Message.Chat,
                photo: new InputFile(image.Encode().AsStream()),
                replyToMessageId: callbackQuery.Message.MessageId,
                replyMarkup: MarkupGenerator.Instance.ScoreKeyboardMarkup(beatmapId, score.Beatmapset.Id),
                cancellationToken: cancellationToken);

            image.Dispose();

            return CallbackResult.Success();
        }
    }
}
