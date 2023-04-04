// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using LiteDB;
using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using osu_bot.Modules.Converters;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Callbacks
{
    public class TopConferenceCallback : ICallback
    {
        public const string DATA = "Top conf";
        public string Data => DATA;

        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;

        public async Task<CallbackResult?> ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data is null)
                return null;

            if (callbackQuery.Message is null)
                return null;
            
            string data = callbackQuery.Data;
            Match beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);

            if (!beatmapIdMatch.Success)
                return new CallbackResult("При обработке запроса произошла ошибка считывания ID карты");
            
            int beatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);

            List<TelegramUser> telegramUsers = _database.TelegramUsers
                .Find(u => u.ChatId == callbackQuery.Message.Chat.Id)
                .ToList();

            List<OsuScore> result = new();

            foreach(TelegramUser telegramUser in telegramUsers)
            {
                IList<OsuScore>? scores = await _service.GetUserBeatmapAllScoresAsync(beatmapId, telegramUser.OsuUser.Id);
                if (scores is not null)
                    result.AddRange(scores);
            }

            if (result.Count == 0)
                return new CallbackResult($"У игроков отсутствуют скоры на карте {beatmapId}");

            using SKImage image = await ImageGenerator.Instance.CreateTableScoresCardAsync(result);

            await botClient.SendPhotoAsync(
                chatId: callbackQuery.Message.Chat,
                photo: new InputFile(image.Encode().AsStream()),
                replyToMessageId: callbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);

            return CallbackResult.Empty();
        }
    }
}
