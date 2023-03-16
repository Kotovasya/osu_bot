// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu_bot.API.Queries;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Callbacks
{
    public enum RequestAction
    {
        Create,
        Edit,
        Cancel,
        Delete,
        Save,
        SelectUser,
        AddUser,
        NextPage,
        PrevPage,
        RequireChange,
        Ok,
    }

    public class RequestCallback : ICallback
    {
        public const string DATA = "Request";

        public string Data => DATA;

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        private readonly Dictionary<long, Request> _editableRequests = new();

        private readonly Dictionary<RequestAction, Action<Request>> _actions;

        private readonly ScoreQuery _scoreQury = new();

        public RequestCallback()
        {
            _actions = new()
            {
                { RequestAction.Create, (request) => _editableRequests.Add(request.Id, request) },
                { RequestAction.Edit, (request) => _editableRequests.Add(request.Id, request) },
                { RequestAction.Cancel, (request) => _editableRequests.Remove(request.Id) },
                { RequestAction.Delete, (request) => _database.Requests.Delete(request.Id) },
                { RequestAction.Save, (request) => _database.Requests.Upsert(request) }
            };
        }

        private Request ChangeRequireFromData(Request request, string data)
        {
            Match requireMatch = new Regex(@"require: (\S+) value: (\S+)$").Match(data);
            if (!requireMatch.Success)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            string propertyName = requireMatch.Groups[0].Value;
            string propertyValue = requireMatch.Groups[1].Value;

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

        private string GetCallbackData(long requestId, string propertyName, object newValue)
        {
            return $"Request id: {requestId} action: {RequestAction.RequireChange} require: {propertyName} value: {newValue}";
        }

        private InlineKeyboardMarkup CreateMarkup(RequestAction action, Request request, string callbackQueryData)
        {
            return action switch
            {
                RequestAction.Cancel => Extensions.KeyboardMarkupForMap(request.ScoreInfo.Id, request.ScoreInfo.BeatmapId),
                RequestAction.Save => Extensions.KeyboardMarkupForMap(request.ScoreInfo.Id, request.ScoreInfo.BeatmapId),
                RequestAction.Edit => CreateEditMarkup(request),
                RequestAction.Create => CreateUserSelectMarkup(request, callbackQueryData)
            };
        }

        private InlineKeyboardMarkup CreateEditMarkup(Request request)
        {
            return new InlineKeyboardMarkup(
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: request.RequirePass ? "Pass ✅" : "Pass ❌",
                        callbackData: GetCallbackData(request.Id, nameof(request.RequirePass), !request.RequirePass)),

                }
            );
        }

        private InlineKeyboardMarkup CreateUserSelectMarkup(Request request, string data)
        {
            Match requestMatch = new Regex(@"page: (\d+)").Match(data);
            if (!requestMatch.Success)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            int page = int.Parse(requestMatch.Groups[0].Value);

            IEnumerable<TelegramUser> users = _database.TelegramUsers.FindAll();
            users = users.Skip((page - 1) * 10).Take(10);
        }

        public async Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return;

            if (callbackQuery.Message == null)
                return;

            string data = callbackQuery.Data;

            Match requestMatch = new Regex(@"Request id: (\d+) action: (\d+)").Match(data);
            if (!requestMatch.Success)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            RequestAction actionRequest = (RequestAction)int.Parse(requestMatch.Groups[0].Value);
            long requestId = long.Parse(requestMatch.Groups[1].Value);

            Request request = actionRequest switch
            {
                RequestAction.Create => new Request(requestId),                
                RequestAction.Edit => _database.Requests.FindById(requestId),
                RequestAction.RequireChange => ChangeRequireFromData(_editableRequests[requestId], data),
                _ => _editableRequests[requestId]
            };

            if (actionRequest == RequestAction.Create)
            {
                TelegramUser requestOwner = _database.TelegramUsers.FindById(callbackQuery.From.Id);
                _scoreQury.Parameters.Username = requestOwner.OsuName;
                _scoreQury.Parameters.ScoreId = requestId;
                request.ScoreInfo = new ScoreInfo(await _scoreQury.ExecuteAsync());
            }

            if (callbackQuery.From.Id != request.FromUser.Id)
                return;

            if (_actions.TryGetValue(actionRequest, out Action<Request>? action))
                action.Invoke(request);

            InlineKeyboardMarkup markup = CreateMarkup(actionRequest, request, data);

            await botClient.EditMessageReplyMarkupAsync(
                chatId: callbackQuery.Message.Chat,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: markup,
                cancellationToken: cancellationToken);
        }
    }
}
