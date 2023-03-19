//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.InteropServices;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using osu_bot.API.Queries;
//using osu_bot.Entites.Database;
//using osu_bot.Modules;
//using Telegram.Bot;
//using Telegram.Bot.Requests.Abstractions;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.ReplyMarkups;

//namespace osu_bot.Bot.Callbacks
//{
//    public enum RequestActionExtension
//    {
//        Create,
//        Edit,
//        Cancel,
//        Delete,
//        Save,
//        PageChange,
//        RequireChange,
//    }

//    public class RequestCallbackExtension : ICallback
//    {
//        public const string DATA = "Request";

//        public string Data => DATA;

//        private readonly DatabaseContext _database = DatabaseContext.Instance;

//        private readonly Dictionary<long, RequestExtension> _editableRequests = new();

//        private readonly Dictionary<RequestActionExtension, Action<RequestExtension>> _actions;

//        private readonly ScoreQuery _scoreQuery = new();

//        public RequestCallbackExtension()
//        {
//            _actions = new()
//            {
//                { RequestActionExtension.Create, (request) => _editableRequests.Add(request.Id, request) },
//                { RequestActionExtension.Edit, (request) => _editableRequests.Add(request.Id, request) },
//                { RequestActionExtension.Cancel, (request) => _editableRequests.Remove(request.Id) },
//                { RequestActionExtension.Delete, (request) => _database.Requests.Delete(request.Id) },
//                { RequestActionExtension.Save, (request) => _database.Requests.Upsert(request) }
//            };
//        }

//        private RequestExtension ChangeRequireFromData(RequestExtension request, string data)
//        {
//            Match requireMatch = new Regex(@"require: (\S+) value: (\S+)$").Match(data);
//            if (!requireMatch.Success)
//                throw new Exception("При обработке запроса на реквест произошла ошибка");

//            string propertyName = requireMatch.Groups[1].Value;
//            string propertyValue = requireMatch.Groups[2].Value;

//            Type requestType = request.GetType();
//            PropertyInfo? propertyInfo = requestType.GetProperty(propertyName);
//            if (propertyInfo == null)
//                throw new Exception("При обработке запроса на реквест произошла ошибка");

//            object value = propertyInfo.PropertyType.Name switch
//            {
//                nameof(Single) => float.Parse(propertyValue),
//                nameof(Int32) => int.Parse(propertyValue),
//                nameof(Int64) => long.Parse(propertyValue),
//                nameof(Boolean) => bool.Parse(propertyValue),
//                nameof(TelegramUser) => _database.TelegramUsers.FindById(long.Parse(propertyValue)),
//                _ => throw new Exception("При обработке запроса на реквест произошла ошибка")
//            };

//            propertyInfo.SetValue(request, value);
//            return request;
//        }

//        private string GetCallbackData(long requestId, string propertyName, object? newValue)
//        {
//            return $"Request id: {requestId} action: {RequestActionExtension.RequireChange} require: {propertyName} value: {newValue}";
//        }

//        private InlineKeyboardMarkup CreateMarkup(RequestActionExtension action, RequestExtension request, string callbackQueryData)
//        {
//            return action switch
//            {
//                RequestActionExtension.Edit => CreateEditMarkup(request),
//                RequestActionExtension.RequireChange => CreateEditMarkup(request),
//                RequestActionExtension.Create => CreateUserSelectMarkup(request, callbackQueryData),
//                RequestActionExtension.PageChange => CreateUserSelectMarkup(request, callbackQueryData),
//                _ => Extensions.KeyboardMarkupForMap(request.ScoreInfo.Id, request.ScoreInfo.BeatmapId)
//            };
//        }

//        private InlineKeyboardMarkup CreateUserSelectMarkup(RequestExtension request, string data)
//        {
//            Match requestMatch = new Regex(@"page: (\d+)").Match(data);
//            if (!requestMatch.Success)
//                throw new Exception("При обработке запроса на реквест произошла ошибка");

//            int page = int.Parse(requestMatch.Groups[0].Value);

//            IEnumerable<TelegramUser> users = _database.TelegramUsers.FindAll();
//            int pagesCount = users.Count() / 8 + 1;
//            users = users.Skip((page - 1) * 8).Take(8);
//            IEnumerator<TelegramUser> usersEnumerator = users.GetEnumerator();
//            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
//            for (int i = 0; i < 4; i++)
//            {
//                List<InlineKeyboardButton> rowButtons = new();
//                int j = 0;
//                while (j < 2 && usersEnumerator.MoveNext())
//                {
//                    TelegramUser user = usersEnumerator.Current;
//                    rowButtons.Add(InlineKeyboardButton.WithCallbackData(
//                        text: user.OsuName,
//                        callbackData: GetCallbackData(request.Id, nameof(request.ToUser), user.Id)));
//                    j++;
//                }
//                keyboard.Add(rowButtons);
//            }

//            List<InlineKeyboardButton> buttons = new();
//            if (page != 1)
//                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️", $"Request id: {request.Id} action: {RequestActionExtension.PageChange} page: {page - 1}"));
//            else
//                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️"));

//            buttons.Add(InlineKeyboardButton.WithCallbackData($"Page {page + 1}/{pagesCount}"));

//            if (page != pagesCount)
//                buttons.Add(InlineKeyboardButton.WithCallbackData("▶️", $"Request id: {request.Id} action: {RequestActionExtension.PageChange} page: {page + 1}"));
//            else
//                buttons.Add(InlineKeyboardButton.WithCallbackData("▶️"));

//            keyboard.Add(buttons);

//            return new InlineKeyboardMarkup(keyboard);
//        }

//        private InlineKeyboardMarkup CreateEditMarkup(RequestExtension request)
//        {
//            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
//            List<InlineKeyboardButton> rowButtons1 = new()
//            {
//                InlineKeyboardButton.WithCallbackData(
//                        text: request.RequirePass ? "Pass✅" : "Pass❌",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequirePass), !request.RequirePass)),
//                InlineKeyboardButton.WithCallbackData(
//                        text: request.RequireFullCombo ? "FC✅" : "FC❌",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireFullCombo), !request.RequireFullCombo))
//            };

//            keyboard.Add(rowButtons1);

//            List<InlineKeyboardButton> rowButtons2;
//            if (request.RequireCompletion != null)
//            {
//                rowButtons2 = new()
//                {
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "-5",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCompletion), request.RequireCompletion - 5)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "-1",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCompletion), request.RequireCompletion - 1)),
//                    InlineKeyboardButton.WithCallbackData($"{request.RequireCompletion:F2}%"),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "+1",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCompletion), request.RequireCompletion + 1)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "+5",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCompletion), request.RequireCompletion + 5)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "❌",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCompletion), null)),
//                };
//            }
//            else
//            {
//                rowButtons2 = new()
//                {
//                    InlineKeyboardButton.WithCallbackData("Completion: not specified"),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "Set value✅",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCompletion), Math.Round(request.ScoreInfo.Completion, MidpointRounding.AwayFromZero))),
//                };
//            }

//            keyboard.Add(rowButtons2);

//            List<InlineKeyboardButton> rowButtons3;
//            if (request.RequireAccuracy != null)
//            {
//                rowButtons3 = new()
//                {
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "-5",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireAccuracy), request.RequireAccuracy - 5)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "-1",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireAccuracy), request.RequireAccuracy - 1)),
//                    InlineKeyboardButton.WithCallbackData($"{request.RequireAccuracy:F2}%"),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "+1",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireAccuracy), request.RequireAccuracy + 1)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "+5",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireAccuracy), request.RequireAccuracy + 5)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "❌",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireAccuracy), null)),
//                };
//            }
//            else
//            {
//                rowButtons3 = new()
//                {
//                    InlineKeyboardButton.WithCallbackData("Accuracy: not specified"),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "Set value✅",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireAccuracy), Math.Round(request.ScoreInfo.Accuracy, MidpointRounding.AwayFromZero))),
//                };
//            }
//            keyboard.Add(rowButtons3);

//            List<InlineKeyboardButton> rowButtons4;
//            if (request.RequireAccuracy != null)
//            {
//                rowButtons4 = new()
//                {
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "-100",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCombo), request.RequireCombo - 5)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "-10",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCombo), request.RequireCombo - 10)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "-1",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCombo), request.RequireCombo - 1)),
//                    InlineKeyboardButton.WithCallbackData($"{request.RequireCombo}/{request.ScoreInfo.MaxCombo}x"),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "+1",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCombo), request.RequireCombo + 1)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "+10",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCombo), request.RequireCombo + 10)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "+100",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCombo), request.RequireCombo + 100)),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "❌",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCombo), null)),
//                };
//            }
//            else
//            {
//                rowButtons4 = new()
//                {
//                    InlineKeyboardButton.WithCallbackData("Accuracy: not specified"),
//                    InlineKeyboardButton.WithCallbackData(
//                        text: "Set value✅",
//                        callbackData: GetCallbackData(request.Id, nameof(request.RequireCombo), request.ScoreInfo.MaxCombo)),
//                };
//            }
//            keyboard.Add(rowButtons4);

//            keyboard.Add(new InlineKeyboardButton[]
//            {
//                InlineKeyboardButton.WithCallbackData(
//                        text: "Save✅",
//                        callbackData: $"Request id: {request.Id} action: {RequestActionExtension.Save}"),
//                InlineKeyboardButton.WithCallbackData(
//                        text: "Cancel❌",
//                        callbackData: $"Request id: {request.Id} action: {RequestActionExtension.Cancel}")
//            });

//            return new InlineKeyboardMarkup(keyboard);
//        }

//        public async Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
//        {
//            if (callbackQuery.Data == null)
//                return;

//            if (callbackQuery.Message == null)
//                return;

//            string data = callbackQuery.Data;

//            Match requestMatch = new Regex(@"Request id: (\d+) action: (\w+)").Match(data);
//            if (!requestMatch.Success)
//                throw new Exception("При обработке запроса на реквест произошла ошибка");

            
            
//            long requestId = long.Parse(requestMatch.Groups[1].Value);
//            RequestActionExtension actionRequest = (RequestActionExtension)Enum.Parse(typeof(RequestActionExtension), requestMatch.Groups[2].Value);

//            RequestExtension request = actionRequest switch
//            {
//                RequestActionExtension.Create => new RequestExtension(requestId),                
//                RequestActionExtension.Edit => _database.Requests.FindById(requestId),
//                RequestActionExtension.RequireChange => ChangeRequireFromData(_editableRequests[requestId], data),
//                _ => _editableRequests[requestId]
//            };

//            requestId = request.Id;

//            if (actionRequest == RequestActionExtension.Create)
//            {
//                TelegramUser requestOwner = _database.TelegramUsers.FindById(callbackQuery.From.Id);
//                _scoreQuery.Parameters.Username = requestOwner.OsuName;
//                _scoreQuery.Parameters.ScoreId = requestId * -1;
//                request.ScoreInfo = new ScoreInfo(await _scoreQuery.ExecuteAsync());
//            }

//            if (callbackQuery.From.Id != request.FromUser.Id)
//                return;

//            if (_actions.TryGetValue(actionRequest, out Action<RequestExtension>? action))
//                action.Invoke(request);

//            InlineKeyboardMarkup markup = CreateMarkup(actionRequest, request, data);

//            await botClient.EditMessageReplyMarkupAsync(
//                chatId: callbackQuery.Message.Chat,
//                messageId: callbackQuery.Message.MessageId,
//                replyMarkup: markup,
//                cancellationToken: cancellationToken);

//            await botClient.AnswerCallbackQueryAsync(
//                callbackQueryId: callbackQuery.Id,
//                cancellationToken: cancellationToken);
//        }
//    }
//}
