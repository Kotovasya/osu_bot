// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
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

            ReplayInfo? replayInfo = null;

            if (long.TryParse(hash, out long scoreId))
                replayInfo = _database.Replays.FindOne(r => r.ScoreId == scoreId);
            else
                replayInfo = _database.Replays.FindById(hash);

            if (replayInfo is not null && replayInfo.ReplayUrl is not null)
                return new CallbackResult(ALREADY_EXIST);

            OsuReplay? replay = await _service.DownloadReplayAsync(hash);

            if (replay is null)
                return new CallbackResult("Невозможно загрузить реплей");        

            return CallbackResult.Success();
        }
    }
}
