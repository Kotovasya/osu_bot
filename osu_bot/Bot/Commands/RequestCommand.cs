// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.API;
using osu_bot.Entites.Database;
using osu_bot.Exceptions;
using osu_bot.Modules;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace osu_bot.Bot.Commands
{
    public class RequestCommand : ICommand
    {
        public string CommandText => "/req";

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text == null)
                return;

            if (message.From == null)
                return;

            TelegramUser user = _database.TelegramUsers
                .Include(u => u.OsuUser)
                .FindById(message.From.Id);

            if (user is null)
                throw new UserNotRegisteredException();

            List<Request> requests = _database.Requests
                .Include(r => r.FromUser)
                .Include(r => r.FromUser.OsuUser)
                .Include(r => r.ToUser)
                .Include(r => r.ToUser.OsuUser)
                .Include(r => r.Beatmap)
                .Include(r => r.Beatmap.Beatmapset)
                .Include(r => r.BeatmapAttributes)
                .Find(r => r.ToUser.Id == user.Id)
                .ToList();

            if (!requests.Any())
                throw new UserRequestsNotFoundException(user.OsuUser.Username);

            int[] requestsId = new int[3];

            if (requests.Count > 1)
                requestsId[2] = requests[1].Id;

            requestsId[1] = requests[0].Id;

            Request request = requests[0];

            SKImage image = await ImageGenerator.Instance.CreateRequestCardAsync(request);

            bool isDelete = !request.RequireSnipe;

            await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputOnlineFile(image.Encode().AsStream()),
                replyToMessageId: message.MessageId,
                replyMarkup: MarkupGenerator.Instance.RequestsKeyboardMarkup(requestsId, 0, requests.Count, isDelete),
                cancellationToken: cancellationToken);
        }
    }
}
