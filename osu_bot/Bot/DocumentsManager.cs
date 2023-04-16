// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using osu_bot.Bot.Commands;
using osu_bot.Bot.Documents;

namespace osu_bot.Bot
{
    public class DocumentsManager
    {
        private static readonly IDocument[] s_documents =
        {
            new ReplayFileHandler(),
        };

        private readonly Dictionary<string, Func<ITelegramBotClient, Message, Stream, CancellationToken, Task>> _documentHandlers = new();

        public DocumentsManager()
        {
            foreach (IDocument document in s_documents)
                _documentHandlers.Add(document.FileExtension, document.ActionAsync);
        }



        public async Task HandlingAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Document is null)
                return;

            Document document = message.Document;
            if (document.FileName is null)
                return;

            int dotIndex = document.FileName.LastIndexOf('.');
            if (dotIndex == -1)
                return;

            string documentExtension = document.FileName[dotIndex..];

            if (_documentHandlers.ContainsKey(documentExtension))
            {
                using MemoryStream stream = new();
                await botClient.GetInfoAndDownloadFileAsync(document.FileId, stream, cancellationToken);
                stream.Position = 0;
                await _documentHandlers[documentExtension].Invoke(botClient, message, stream, cancellationToken);
            }
        }
    }
}
