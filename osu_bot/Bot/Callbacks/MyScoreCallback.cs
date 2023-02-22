using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace osu_bot.Bot.Callbacks
{
    public class MyScoreCallback : Callback
    {
        public override string Data => "Beatmap score";

        public BeatmapScoresQuery query = new();

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery.Data == null)
                return;

            var parameters = new BeatmapScoresQueryParameters();
            query.Parameters = parameters;

            var telegramUser = Database.TelegramUsers.FindOne(u => u.Id == update.Message.From.Id);
            if (telegramUser != null)
                parameters.Username = telegramUser.OsuName;
            else
                throw new Exception("Аккаунт Osu не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");

            var data = update.CallbackQuery.Data;
            var beatmapIdMatch = new Regex(@"beatmapId(\d+)").Match(data);

            if (beatmapIdMatch.Success)
                query.Parameters.BeatmapId = int.Parse(beatmapIdMatch.Groups[1].Value);

            var scores = await query.ExecuteAsync(API);
        }
    }
}
