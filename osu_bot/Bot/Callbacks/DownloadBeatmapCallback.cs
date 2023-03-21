// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu_bot.API;
using osu_bot.API.Queries;
using osu_bot.Entites;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace osu_bot.Bot.Callbacks
{
    public class DownloadBeatmapCallback : ICallback
    {
        public const string DATA = "Download";

        public string Data => DATA;

        private readonly BeatmapInfoQuery _beatmapInfoQuery = new();

        public async Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return;

            if (callbackQuery.Message == null)
                return;

            string data = callbackQuery.Data;

            Match beatmapMatch = new Regex(@"Download B: (\d+)").Match(data);
            if (!beatmapMatch.Success)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            long beatmapId = long.Parse(beatmapMatch.Groups[1].Value);

            _beatmapInfoQuery.Parameters.BeatmapId = beatmapId;
            OsuBeatmap beatmap = await _beatmapInfoQuery.ExecuteAsync();
            Stream fileStream = await OsuAPI.Instance.BeatmapsetDownloadAsync(beatmap.BeatmapsetId);

            await botClient.SendDocumentAsync(
                chatId: callbackQuery.Message.Chat,
                document: new InputOnlineFile(fileStream, $"{beatmap.Title} - {beatmap.Artist}.osz"),
                replyToMessageId: callbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);
        }
    }
}
