// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Entites;
using osu_bot.Modules;
using osu_bot.Modules.OsuFiles;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Formats.Asn1.AsnWriter;
using Telegram.Bot.Types.ReplyMarkups;
using osu_bot.Entites.Database;

namespace osu_bot.Bot.Documents
{
    public class ReplayFileHandler : IDocument
    {
        public string FileExtension => ".osr";

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, Stream replayDataStream, CancellationToken cancellationToken)
        {
            if (message.Document is null)
                return;

            OsuReplay replay = ReplayReader.FromStream(replayDataStream);
            OsuScore? score = await replay.ToOsuScore();

            if (score is null)
                throw new NotImplementedException();

            long? scoreId = replay.OnlineScoreId is -1 or 0 ? replay.OnlineScoreId : null;
            string fileName = scoreId is null ? replay.ReplayHash : replay.OnlineScoreId.ToString();

            if (!_database.FileStorage.Exists(replay.ReplayHash))
                _database.FileStorage.Upload(replay.ReplayHash, $"{fileName}.osr", replayDataStream);

            if (scoreId is not null && !_database.Replays.Exists(replay.ReplayHash))
                _database.Replays.Insert(new ReplayUpload(replay.ReplayHash, replay.OnlineScoreId));

            SKImage image = await ImageGenerator.Instance.CreateFullCardAsync(score);
            InlineKeyboardMarkup inlineKeyboard = MarkupGenerator.Instance.ScoreKeyboardMarkup(score.Beatmap.Id, score.Beatmapset.Id, replay.ReplayHash);

            await botClient.SendPhotoAsync(
                chatId: message.Chat,
                photo: new InputFile(image.Encode().AsStream()),
                replyToMessageId: message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
