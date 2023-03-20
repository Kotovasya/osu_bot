// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using LiteDB;
using osu_bot.API;
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
    public class TopConferenceCallback : ICallback
    {
        public const string DATA = "Top conf";
        public string Data => DATA;

        private readonly BeatmapScoresQuery _beatmapScoresQuery = new();
        private readonly BeatmapInfoQuery _beatmapInfoQuery = new();
        private readonly BeatmapAttributesJsonQuery _beatmapAttributesQuery = new();

        private readonly OsuAPI _api = OsuAPI.Instance;
        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
            {
                return;
            }

            if (callbackQuery.Message == null)
            {
                return;
            }

            string data = callbackQuery.Data;
            Match beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);

            if (!beatmapIdMatch.Success)
            {
                throw new Exception("При обработке запроса \"Топ конфы\" произошла ошибка считывания ID карты");
            }

            int beatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);

            _beatmapInfoQuery.Parameters.BeatmapId = beatmapId;
            OsuBeatmap beatmap = await _beatmapInfoQuery.ExecuteAsync();
            IEnumerable<Mod>? mods = ModsConverter.ToMods(new string[] { NoMod.NAME } );

            _beatmapAttributesQuery.Parameters.BeatmapId = beatmapId;
            _beatmapAttributesQuery.Parameters.Mods = mods;
            beatmap.Attributes.ParseDifficultyAttributesJson(await _beatmapAttributesQuery.ExecuteAsync());

            List<OsuScoreInfo> result = new();
            List<TelegramUser> telegramUsers = _database.TelegramUsers.FindAll().ToList();

            foreach (TelegramUser telegramUser in telegramUsers)
            { 
                IEnumerable<OsuScoreInfo> scores;

                if (beatmap.ScoresTable)
                {
                    //После выполнения ExecuteAsync beatmapScoreQuery.Parameters = new()
                    _beatmapScoresQuery.Parameters.BeatmapId = beatmapId;
                    _beatmapScoresQuery.Parameters.Username = telegramUser.OsuName;
                    scores = await _beatmapScoresQuery.ExecuteAsync();
                }
                else
                {
                    scores = _database.Scores
                        .Find(s => s.BeatmapId == beatmapId)
                        .Where(s => s.User.Id == telegramUser.Id)
                        .Select(s => new OsuScoreInfo(s))
                        .ToList();

                    OsuUser user = await _api.GetUserInfoByUsernameAsync(telegramUser.OsuName);
                    foreach (OsuScoreInfo score in scores)
                        score.User = user;
                }

                foreach (OsuScoreInfo score in scores)
                    score.Beatmap = beatmap;

                result.AddRange(scores);
            }

            if (result.Count == 0)
                throw new Exception($"У игроков отсутствуют скоры на карте {beatmapId}");

            SKImage image = await ImageGenerator.Instance.CreateTableScoresCardAsync(result);

            await botClient.SendPhotoAsync(
                chatId: callbackQuery.Message.Chat,
                photo: new InputOnlineFile(image.Encode().AsStream()),
                replyToMessageId: callbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);
        }
    }
}
