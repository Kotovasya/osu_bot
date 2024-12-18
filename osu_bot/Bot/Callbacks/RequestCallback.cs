﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Text.RegularExpressions;
using osu_bot.API;
using osu_bot.API.Handlers;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using osu_bot.Modules.Converters;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Callbacks
{
    public enum RequestCallbackAction
    {
        Create,
        Cancel,
        Delete,
        Save,
        PageChange,
        RequireChange,
        Snipe,
        SnipeSelect,
        SnipeCancel,
        SnipeRequireCancel,
        SRC //Snipe Require Change
    }

    public class RequestCallback : ICallback
    {
        public const string DATA = "RC";

        public string Data => DATA;

        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;

        private readonly Dictionary<RequestCallbackAction, Action<Request>> _actions;

        public RequestCallback()
        {
            _actions = new()
            {
                { RequestCallbackAction.Create, (request) => _database.Requests.Insert(request) },
                { RequestCallbackAction.Cancel, (request) => _database.Requests.Delete(request.Id) },
                { RequestCallbackAction.Delete, (request) => _database.Requests.Delete(request.Id) },  
                { RequestCallbackAction.RequireChange, (request) => _database.Requests.Update(request) },
                { RequestCallbackAction.Snipe, (request) => _database.Requests.Update(request) },
                { RequestCallbackAction.SRC, (request) => _database.Requests.Update(request) },
            };
        }

        private Request ChangeRequireFromData(Request request, string data)
        {
            Match requireMatch = new Regex(@"R:(\S+) V:(\S+)$").Match(data);
            if (!requireMatch.Success)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            string propertyName = requireMatch.Groups[1].Value;
            string propertyValue = requireMatch.Groups[2].Value;

            Type requestType = request.GetType();
            PropertyInfo? propertyInfo = requestType.GetProperty(propertyName);
            if (propertyInfo == null)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            object value = propertyInfo.PropertyType.Name switch
            {
                nameof(Single) => float.Parse(propertyValue),
                nameof(Int32) => int.Parse(propertyValue),
                nameof(Int64) => long.Parse(propertyValue),
                nameof(Boolean) => bool.Parse(propertyValue),
                nameof(TelegramUser) => _database.TelegramUsers.FindById(long.Parse(propertyValue)),
                _ => throw new Exception("При обработке запроса на реквест произошла ошибка")
            };

            propertyInfo.SetValue(request, value);
            return request;
        }

        public async Task<CallbackResult?> ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
           if (callbackQuery.Data == null)
                return null;

            if (callbackQuery.Message == null)
                return null;

            string data = callbackQuery.Data;

            Match requestMatch = new Regex(@"RC:(\d+) A:(\w+)").Match(data);
            if (!requestMatch.Success)
                return new CallbackResult("При обработке запроса на реквест произошла ошибка", 500);

            int requestId = int.Parse(requestMatch.Groups[1].Value);
            RequestCallbackAction actionRequest = (RequestCallbackAction)Enum.Parse(typeof(RequestCallbackAction), requestMatch.Groups[2].Value);

            Request? request;
            if (actionRequest is RequestCallbackAction.Create)
            {
                OsuBeatmap? beatmap = await _service.GetBeatmapAsync(requestId);

                if (beatmap is null)
                    return new CallbackResult("При обработке запроса на реквест произошла ошибка", 500);

                _database.Beatmaps.Upsert(beatmap);
                _database.Beatmapsets.Upsert(beatmap.Beatmapset);

                TelegramUser fromUser = _database.TelegramUsers.FindById(callbackQuery.From.Id);
                request = new Request(fromUser, beatmap);
            }
            else if (actionRequest is RequestCallbackAction.Save)
            {
                request = _database.Requests
                    .Include(r => r.FromUser)
                    .Include(r => r.FromUser.OsuUser)
                    .Include(r => r.ToUser)
                    .Include(r => r.ToUser.OsuUser)
                    .Include(r => r.Beatmap)
                    .Include(r => r.Beatmap.Beatmapset)
                    .Include(r => r.BeatmapAttributes)
                    .FindById(requestId);
            }
            else
            {
                request = _database.Requests
                    .Include(u => u.FromUser)
                    .FindById(requestId);
            }

            if (request is null)
                return new CallbackResult("При обработке запроса на реквест произошла ошибка", 500);

            if (callbackQuery.From.Id != request.FromUser.Id)
                return null;

            if (actionRequest is RequestCallbackAction.RequireChange || actionRequest is RequestCallbackAction.SRC)
            {
                try
                {
                    request = ChangeRequireFromData(request, data);
                }
                catch (Exception)
                {
                    return new CallbackResult("При обработке запроса на реквест произошла ошибка", 500);
                }
            }

            if (actionRequest is RequestCallbackAction.Snipe)
            {
                Match scoreMatch = new Regex(@"M:(\d+) S:(\d+) C:(\d+) F:(\d+\D\d+)").Match(data);
                if (!scoreMatch.Success)
                    return new CallbackResult("При обработке запроса на реквест произошла ошибка", 500);

                int mods = int.Parse(scoreMatch.Groups[1].Value);
                int score = int.Parse(scoreMatch.Groups[2].Value);
                int combo = int.Parse(scoreMatch.Groups[3].Value);
                float accuracy = float.Parse(scoreMatch.Groups[4].Value);

                request.Score = score;
                request.Accuracy = accuracy;
                request.Combo = combo;
                request.RequireSnipe = true;
                request.IsOnlyMods = true;
                request.RequireMods = mods; 
            }

            if (actionRequest is RequestCallbackAction.SnipeCancel)
            {
                request.RequirePass = true;
                request.IsOnlyMods = false;
            }

            if (actionRequest is RequestCallbackAction.Save)
            {
                if (request.RequireMods == 0)
                    return new CallbackResult("Должен быть выбран хотя бы один из модов");

                IEnumerable<OsuScore>? scores = await _service.GetUserBeatmapAllScoresAsync(request.Beatmap.Id, request.ToUser.OsuUser.Id);

                if (scores != null)
                {
                    if (scores.Any(s => RequestsHandler.CheckRequestComplete(s, request)))
                        return new CallbackResult("У игрока на карте уже имеется скор, выполняющий этот реквест");
                }

                IEnumerable<Request> similarRequests = _database.Requests.Find(r =>
                    r.Beatmap.Id == request.Beatmap.Id
                    && r.ToUser.Id == request.ToUser.Id
                    && r.IsOnlyMods == request.IsOnlyMods
                    && !r.IsTemporary && !r.IsComplete);

                foreach (Request similarRequest in similarRequests)
                {
                    if (similarRequest.IsOnlyMods)
                    {
                        if (similarRequest.RequireMods != request.RequireMods)
                            continue;
                    }
                    else
                    {
                        IEnumerable<Mod>? similarMods = ModsConverter.ToMods(similarRequest.RequireMods);
                        IEnumerable<Mod>? requestMods = ModsConverter.ToMods(request.RequireMods);

                        if (!requestMods.Any(m => similarMods.Contains(m)))
                            continue;
                    }
                    bool isSimilarRequest = false;
                    if (similarRequest.RequirePass && request.RequirePass)
                        isSimilarRequest = true;
                    else if (similarRequest.RequireFullCombo && request.RequireFullCombo)
                        isSimilarRequest = true;
                    else if (similarRequest.RequireSnipe && request.RequireSnipe)
                    {
                        if (request.RequireSnipeAcc && request.RequireSnipeAcc)
                            isSimilarRequest = true;
                        else if (request.RequireSnipeCombo && request.RequireSnipeCombo)
                            isSimilarRequest = true;
                        else if (request.RequireSnipeScore && request.RequireSnipeScore)
                            isSimilarRequest = true;
                    }

                    if (isSimilarRequest)
                        return new CallbackResult("Похожий реквест для этого игрока уже существует");
                }
            }

            if (_actions.TryGetValue(actionRequest, out Action<Request>? action))
                action.Invoke(request);

            InlineKeyboardMarkup newReplyMarkup =
                await MarkupGenerator.Instance.CreateRequestCallbackMarkup(actionRequest, request, data, callbackQuery.Message.Chat.Id);

            try
            {
                await botClient.EditMessageReplyMarkupAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    replyMarkup: newReplyMarkup,
                    messageId: callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("message is not modified"))
                    throw;
            }

            if (actionRequest == RequestCallbackAction.Save)
            {

                ChatMember fromMember = await botClient.GetChatMemberAsync(
                    chatId: callbackQuery.Message.Chat,
                    userId: request.FromUser.Id,
                    cancellationToken: cancellationToken);

                ChatMember toMember = await botClient.GetChatMemberAsync(
                    chatId: callbackQuery.Message.Chat,
                    userId: request.ToUser.Id,
                    cancellationToken: cancellationToken);

                OsuBeatmapAttributes? attributes = null;
                if (request.IsOnlyMods)
                    attributes = await _service.GetBeatmapAttributesAsync(request.Beatmap, request.RequireMods);
                else
                    attributes = await _service.GetBeatmapAttributesAsync(request.Beatmap, NoMod.NUMBER);

                if (attributes is null)
                {
                    attributes = new()
                    {
                        Id = new BeatmapAttributesKey(request.Beatmap.Id, NoMod.NUMBER),
                        Stars = request.Beatmap.Stars
                    };
                    attributes.CopyBeatmapAttributes(request.Beatmap);
                }

                request.IsTemporary = false;
                request.BeatmapAttributes = attributes;
                _database.BeatmapAttributes.Upsert(attributes);
                _database.Requests.Update(request);

                using SKImage image = await ImageGenerator.Instance.CreateRequestCardAsync(request);

                Message message = await botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat,
                    photo: new InputFile(image.Encode().AsStream()),
                    caption: $"@{fromMember.User.Username} создал реквест для @{toMember.User.Username} на карте {request.Beatmap.Url}",
                    replyMarkup: MarkupGenerator.Instance.RequestKeyboardMakrup(request),
                    cancellationToken: cancellationToken);

                await botClient.ForwardMessageAsync(
                    chatId: TelegramBot.REQUESTS_THREAD_ID,
                    fromChatId: callbackQuery.Message.Chat,
                    messageId: message.MessageId,
                    messageThreadId: TelegramBot.REQUESTS_THREAD_ID,
                    cancellationToken: cancellationToken);
            }

            return CallbackResult.Success();
        }
    }
}
