// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LiteDB;
using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules.OsuFiles;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Callbacks
{
    public class ReplayCallback : ICallback
    {
        public const string DATA = "Replay";
        public const string ALREADY_EXIST = "Replay already exist";

        public string Data => DATA;

        private readonly OsuService _service = OsuService.Instance;
        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task<CallbackResult?> ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return null;

            if (callbackQuery.Message == null)
                return null;

            string data = callbackQuery.Data;

            Match requestMatch = new Regex(@"Replay id:(\w+)").Match(data);
            if (!requestMatch.Success)
                return new CallbackResult("При обработке запроса на реплей произошла ошибка", 500);

            string hash = requestMatch.Groups[1].Value;
            using MemoryStream replayDataStream = new();

            ReplayUpload? replay = _database.Replays.FindById(hash);

            if (replay is null && long.TryParse(hash, out long scoreId))
                replay = _database.Replays.FindOne(r => r.ScoreId == scoreId);

            if (replay is not null && replay.Url is not null)
                return new CallbackResult(ALREADY_EXIST);

            MemoryStream replayData = await _service.GetReplayDataAsync(hash);
            if (replayData.Length == 0)
                return new CallbackResult("Не удалось получить реплей из локальной и osu! БД");

            if (replay is null)
                return new CallbackResult("Невозможно загрузить реплей");        



            return CallbackResult.Success();
        }
    }
}
