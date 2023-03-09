// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using LiteDB;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace osu_bot.Bot.Callbacks
{
    public class TopConferenceCallback : Callback
    {
        public const string DATA = "Top conf";
        public override string Data => DATA;

        private readonly BeatmapScoresQuery _beatmapScoresQuery = new();
        private readonly BeatmapInfoQuery _beatmapInfoQuery = new();
        private readonly BeatmapAttributesJsonQuery _beatmapAttributesQuery = new();

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

            string data = update.CallbackQuery.Data;
            Match beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);

            if (!beatmapIdMatch.Success)
            {
                throw new Exception("При обработке запроса \"Топ конфы\" произошла ошибка считывания ID карты");
            }

            int beatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);

            _beatmapInfoQuery.Parameters.BeatmapId = beatmapId;
            Beatmap beatmap = await _beatmapInfoQuery.ExecuteAsync();
            IEnumerable<Mod> mods = ModsConverter.ToMods(Array.Empty<string>());

            _beatmapAttributesQuery.Parameters.BeatmapId = beatmapId;
            _beatmapAttributesQuery.Parameters.Mods = mods;
            beatmap.Attributes.ParseDifficultyAttributesJson(await _beatmapAttributesQuery.ExecuteAsync());

            List<ScoreInfo> result = new();
            List<TelegramUser> telegramUsers = _database.TelegramUsers.FindAll().ToList();

            foreach (TelegramUser telegramUser in telegramUsers)
            {
                //После выполнения ExecuteAsync beatmapScoreQuery.Parameters = new()
                _beatmapScoresQuery.Parameters.Mods = mods;
                _beatmapScoresQuery.Parameters.BeatmapId = beatmapId;
                _beatmapScoresQuery.Parameters.Username = telegramUser.OsuName;
                List<ScoreInfo> scores = await _beatmapScoresQuery.ExecuteAsync();

                foreach (ScoreInfo score in scores)
                {
                    score.Beatmap = beatmap;
                }

                result.AddRange(scores);
            }

            SKImage image = await ImageGenerator.Instance.CreateTableScoresCardAsync(result);

            _ = await botClient.SendPhotoAsync(
                chatId: update.CallbackQuery.Message.Chat,
                photo: new InputOnlineFile(image.EncodedData.AsStream()),
                replyToMessageId: update.CallbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);
        }
    }
}
