﻿using LiteDB;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Entites.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

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
            beatmap.Attributes.ParseDifficultyAttributesJson(await new BeatmapAttributesQuery().GetJson(API));

            List<ScoreInfo> result = new();

            foreach (var telegramUser in Database.TelegramUsers.FindAll())
            {
                parameters.Username = telegramUser.OsuName;
                var scores = await query.ExecuteAsync(API);

                foreach (var score in scores)
                {
                    var resultScore = result.FirstOrDefault(s => s.User.Name == telegramUser.OsuName
                        && new HashSet<Mod>(s.Mods).SetEquals(score.Mods));
                    if (resultScore == null)
                        result.Add(score);

                    else if (score.Score > resultScore.Score)
                    {
                        result.Add(score);
                        result.Remove(resultScore);  
                    }
                    score.Beatmap = beatmap;
                }
            }
        }
    }
}