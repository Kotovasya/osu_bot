using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace osu_bot.Bot.Callbacks
{
    public class MyScoreCallback : Callback
    {
        public const string DATA = "My score";
        public override string Data => DATA;

        public BeatmapBestScoreQuery query = new();

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery.Data == null)
                return;

            var parameters = new BeatmapBestScoresQueryParamters();
            query.Parameters = parameters;

            var telegramUser = Database.TelegramUsers.FindOne(u => u.Id == update.CallbackQuery.From.Id);
            if (telegramUser != null)
                parameters.Username = telegramUser.OsuName;
            else
                throw new Exception("Аккаунт Osu не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");

            var data = update.CallbackQuery.Data;
            var beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);

            if (!beatmapIdMatch.Success)
                throw new Exception("При обработке запроса \"Мой скор\" произошла ошибка считывания ID карты");

            parameters.BeatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);

            var score = await query.ExecuteAsync(API);

            var image = ImageGenerator.CreateFullCard(score);
            var imageStream = image.ToStream();

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "🎯Мой скор", callbackData: $"{DATA} beatmapId{score.Beatmap.Id})"),
                    InlineKeyboardButton.WithCallbackData(text: "🏆Топ конфы", callbackData: MapsCallback.DATA)
                });


            await botClient.SendPhotoAsync(
                chatId: update.CallbackQuery.Message.Chat,
                caption: score.Beatmap.Url,
                photo: new InputOnlineFile(new MemoryStream(imageStream)),
                replyToMessageId: update.CallbackQuery.Message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
