// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using osu_bot.API;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
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

        private readonly BeatmapBestScoreQuery _beatmapBestScoreQuery = new();
        private readonly BeatmapInfoQuery _beatmapInfoQuery = new();
        private readonly BeatmapAttributesJsonQuery _beatmapAttributesJsonQuery = new();

        private readonly OsuAPI _api = OsuAPI.Instance;
        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return;

            if (callbackQuery.Message == null)
                return;
            

            BeatmapBestScoresQueryParameters parameters = _beatmapBestScoreQuery.Parameters;

            TelegramUser telegramUser = _database.TelegramUsers.FindOne(u => u.Id == callbackQuery.From.Id);
            if (telegramUser == null)
                throw new Exception("Аккаунт Osu не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");

            parameters.Username = telegramUser.OsuName;

            string data = callbackQuery.Data;

            Match beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);
            if (!beatmapIdMatch.Success)
            {
                throw new Exception("При обработке запроса \"Мой скор\" произошла ошибка считывания ID карты");
            }

            parameters.BeatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);

            _beatmapInfoQuery.Parameters.BeatmapId = parameters.BeatmapId;
            OsuBeatmap beatmap = await _beatmapInfoQuery.ExecuteAsync();

            OsuScoreInfo? score;

            if (beatmap.ScoresTable)
            {
                score = await _beatmapBestScoreQuery.ExecuteAsync();
                if (score == null)
                    throw new UserScoreNotFoundException(parameters.Username, parameters.BeatmapId);
            }
            else
            {
                ScoreInfo? scoreInfo = _database.Scores
                    .Find(s => s.BeatmapId == parameters.BeatmapId)
                    .Where(s => s.User.Id == telegramUser.Id)
                    .MaxBy(s => s.Score);

                if (scoreInfo == null)
                    throw new UserScoreNotFoundException(parameters.Username, parameters.BeatmapId);

                score = new OsuScoreInfo(scoreInfo)
                {
                    User = await _api.GetUserInfoByUsernameAsync(telegramUser.OsuName)
                };
            }

#pragma warning disable CS8602
            score.Beatmap = beatmap;
#pragma warning restore CS8602

            _beatmapAttributesJsonQuery.Parameters.BeatmapId = beatmap.Id;
            _beatmapAttributesJsonQuery.Parameters.Mods = score.Mods;
            score.Beatmap.Attributes.ParseDifficultyAttributesJson(await _beatmapAttributesJsonQuery.ExecuteAsync());
            score.Beatmap.Attributes.CalculateAttributesWithMods(score.Mods);           

            SKImage image = await ImageGenerator.Instance.CreateFullCardAsync(score);

            InlineKeyboardMarkup inlineKeyboard = Extensions.ScoreKeyboardMarkup(score.Beatmap.Id);

            await botClient.SendPhotoAsync(
                chatId: callbackQuery.Message.Chat,
                photo: new InputOnlineFile(image.Encode().AsStream()),
                replyToMessageId: callbackQuery.Message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
