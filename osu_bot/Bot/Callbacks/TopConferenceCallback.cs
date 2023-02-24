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

namespace osu_bot.Bot.Callbacks
{
    public class TopConferenceCallback : Callback
    {
        public const string DATA = "Top conf";
        public override string Data => DATA;

        public BeatmapScoresQuery query = new();

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery.Data == null)
                return;

            var parameters = new BeatmapScoresQueryParameters();
            query.Parameters = parameters;

            var data = update.CallbackQuery.Data;
            var beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);

            if (!beatmapIdMatch.Success)
                throw new Exception("При обработке запроса \"Топ конфы\" произошла ошибка считывания ID карты");

            parameters.BeatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);

            var beatmap = await new BeatmapInfoQuery(parameters.BeatmapId).ExecuteAsync(API);
            var attributesQuery = new BeatmapAttributesQuery();
            attributesQuery.Parameters.BeatmapId = parameters.BeatmapId;
            attributesQuery.Parameters.Mods = ModsConverter.ToMods(Array.Empty<string>());
            beatmap.Attributes.ParseDifficultyAttributesJson(await attributesQuery.GetJson(API));

            List<ScoreInfo> result = new();
            var telegramUsers = Database.TelegramUsers.FindAll().ToList();

            foreach (var telegramUser in telegramUsers)
            {
                parameters.Username = telegramUser.OsuName;
                var scores = await query.ExecuteAsync(API);
                foreach (var score in scores)
                    score.Beatmap = beatmap;

                result.AddRange(scores);
            }

            var image = ImageGenerator.CreateTableScoresCard(result);
            var imageStream = image.ToStream();

            await botClient.SendPhotoAsync(
                chatId: update.CallbackQuery.Message.Chat,
                caption: beatmap.Url,
                photo: new InputOnlineFile(new MemoryStream(imageStream)),
                replyToMessageId: update.CallbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);
        }
    }
}
