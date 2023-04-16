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

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, Stream fileStream, CancellationToken cancellationToken)
        {
            if (message.Document is null)
                return;

            OsuReplay replay = ReplayReader.FromStream(fileStream);
            OsuScore? score = await replay.ToOsuScore();

            if (score is null)
                throw new NotImplementedException();

            ReplayInfo replayInfo = replay.ReplayInfo;
            replayInfo.TelegramFileId = message.Document.FileId;
            _database.Replays.Insert(replayInfo);

            SKImage image = await ImageGenerator.Instance.CreateFullCardAsync(score);
            InlineKeyboardMarkup inlineKeyboard = MarkupGenerator.Instance.ScoreKeyboardMarkup(score.Beatmap.Id, score.Beatmapset.Id, replayInfo.ReplayHash);

            await botClient.SendPhotoAsync(
                chatId: message.Chat,
                photo: new InputFile(image.Encode().AsStream()),
                replyToMessageId: message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
