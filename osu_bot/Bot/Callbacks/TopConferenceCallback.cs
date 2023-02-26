using LiteDB;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Formats.Asn1.AsnWriter;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Requests;
using osu_bot.API;
using osu_bot.Entites.Database;
using System.Collections;

namespace osu_bot.Bot.Callbacks
{
    public class TopConferenceCallback : Callback
    {
        public const string DATA = "Top conf";
        public override string Data => DATA;

        private readonly BeatmapScoresQuery beatmapScoresQuery = new();
        private readonly BeatmapInfoQuery beatmapInfoQuery = new();
        private readonly BeatmapAttributesJsonQuery beatmapAttributesQuery = new();

        private readonly DatabaseContext Database = DatabaseContext.Instance;

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery.Data == null)
                return;

            var data = update.CallbackQuery.Data;
            var beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);

            if (!beatmapIdMatch.Success)
                throw new Exception("При обработке запроса \"Топ конфы\" произошла ошибка считывания ID карты");
            var beatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);

            beatmapInfoQuery.Parameters.BeatmapId = beatmapId;
            var beatmap = await beatmapInfoQuery.ExecuteAsync();
            var mods = ModsConverter.ToMods(Array.Empty<string>());

            beatmapAttributesQuery.Parameters.BeatmapId = beatmapId;
            beatmapAttributesQuery.Parameters.Mods = mods;
            beatmap.Attributes.ParseDifficultyAttributesJson(await beatmapAttributesQuery.ExecuteAsync());

            List<ScoreInfo> result = new();
            var telegramUsers = Database.TelegramUsers.FindAll().ToList();

            foreach (var telegramUser in telegramUsers)
            {
                //После выполнения ExecuteAsync beatmapScoreQuery.Parameters = new()
                beatmapScoresQuery.Parameters.Mods = mods;
                beatmapScoresQuery.Parameters.BeatmapId = beatmapId;
                beatmapScoresQuery.Parameters.Username = telegramUser.OsuName;
                var scores = await beatmapScoresQuery.ExecuteAsync();

                foreach (var score in scores)
                    score.Beatmap = beatmap;

                result.AddRange(scores);
            }

            var image = ImageGenerator.CreateTableScoresCard(result);
            var imageStream = image.ToStream();

            await botClient.SendPhotoAsync(
                chatId: update.CallbackQuery.Message.Chat,
                photo: new InputOnlineFile(new MemoryStream(imageStream)),
                replyToMessageId: update.CallbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);
        }
    }
}
