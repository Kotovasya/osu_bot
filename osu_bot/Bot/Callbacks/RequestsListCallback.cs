// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Callbacks
{
    public class RequestsListCallback : ICallback
    {
        public const string DATA = "Requests";

        public string Data => DATA;

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task<CallbackResult?> ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return null;

            if (callbackQuery.Message == null)
                return null;

            string data = callbackQuery.Data;

            if (data.Contains("Hide"))
            {
                await botClient.DeleteMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
                return CallbackResult.Success();
            }

            Match idMatch = new Regex(@"ID:(\d+)").Match(data);
            if (!idMatch.Success)
                return new CallbackResult("При обработке запроса на реквест произошла ошибка");

            int requestId = int.Parse(idMatch.Groups[1].Value);

            Request request = _database.Requests.FindById(requestId);
            if (request is null)
                return new CallbackResult("При обработке запроса на реквест произошла ошибка");
            if (request.ToUser.Id != callbackQuery.From.Id)
                return new CallbackResult("Нельзя редактировать реквест, созданный другим пользователем", 500);       

            List<Request> requests = _database.Requests
                .Include(r => r.FromUser)
                .Include(r => r.FromUser.OsuUser)
                .Include(r => r.ToUser)
                .Include(r => r.ToUser.OsuUser)
                .Include(r => r.Beatmap)
                .Include(r => r.Beatmap.Beatmapset)
                .Include(r => r.BeatmapAttributes)
                .Find(r => r.ToUser.Id == callbackQuery.From.Id)
                .Where(r => !r.IsTemporary && !r.IsComplete)
                .ToList();

            int requestsCount = requests.Count;

            Match pageMatch = new Regex(@"P:(\d+)").Match(data);
            if (!pageMatch.Success)
                throw new NotImplementedException();
            int page = int.Parse(pageMatch.Groups[1].Value);

            if (data.Contains("Delete"))
            {
                _database.Requests.Delete(requestId);
                if (requestsCount - 1 == 0)
                {
                    await botClient.DeleteMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        cancellationToken: cancellationToken);
                    return CallbackResult.Success();
                }
                if (page != requestsCount - 1)
                    page += 1;
                else
                    page -= 1;

                requestsCount -= 1;
            }

            int[] requestsId = new int[3];
            if (page != 0)
                requestsId[0] = requests[page - 1].Id;
            else
                requestsId[0] = requests[requestsCount - 1].Id;

            requestsId[1] = requests[page].Id;

            if (page != requestsCount - 1)
                requestsId[2] = requests[page + 1].Id;
            else
                requestsId[2] = requests[0].Id;

            request = requests[page];

            bool isDelete = !request.RequireSnipe;

            using SKImage image = await ImageGenerator.Instance.CreateRequestCardAsync(request);
            await botClient.EditMessageMediaAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                media: new InputMediaPhoto(new InputFile(image.Encode().AsStream(), page.ToString())),
                replyMarkup: MarkupGenerator.Instance.RequestsKeyboardMarkup(requestsId, page, requestsCount, isDelete),
                cancellationToken: cancellationToken);

            return CallbackResult.Success();
        }
    }
}
