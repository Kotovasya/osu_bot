// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Text.RegularExpressions;
using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using osu_bot.Modules.Converters;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Callbacks
{
    public enum RequestAction
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
        public const string DATA = "R";

        public string Data => DATA;

        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;

        private readonly Dictionary<RequestAction, Action<Request>> _actions;

        public RequestCallback()
        {
            _actions = new()
            {
                { RequestAction.Create, (request) => _database.Requests.Insert(request) },
                { RequestAction.Cancel, (request) => _database.Requests.Delete(request.Id) },
                { RequestAction.Delete, (request) => _database.Requests.Delete(request.Id) },  
                { RequestAction.RequireChange, (request) => _database.Requests.Update(request) },
                { RequestAction.Snipe, (request) => _database.Requests.Update(request) },
                { RequestAction.SRC, (request) => _database.Requests.Update(request) },
            };
        }

        private string GetRequireCallbackData(long requestId, string propertyName, object? newValue)
        {
            return $"{DATA}:{requestId} A:{RequestAction.RequireChange} R:{propertyName} V:{newValue}";
        }

        private string GetSnipeRequireCallbackData(long requestId, string propertyName, object? newValue)
        {
            return $"{DATA}:{requestId} A:{RequestAction.SRC} R:{propertyName} V:{newValue}";
        }

        private InlineKeyboardMarkup CreateUserSelectMarkup(Request request, string data, long chatId)
        {
            Match requestMatch = new Regex(@"P:(\d+)").Match(data);
            if (!requestMatch.Success)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            int page = int.Parse(requestMatch.Groups[1].Value);

            IEnumerable<TelegramUser> users = _database.TelegramUsers
                .Include(u => u.OsuUser)
                .Find(u => u.ChatId == chatId);
            int pagesCount = users.Count() / 8 + 1;
            users = users.Skip((page - 1) * 8).Take(8);
            IEnumerator<TelegramUser> usersEnumerator = users.GetEnumerator();
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Выбери своего бойца:"),
                InlineKeyboardButton.WithCallbackData("❌ Cancel", $"{DATA}:{request.Id} A:{RequestAction.Cancel}")
            });
            for (int i = 0; i < 4; i++)
            {
                List<InlineKeyboardButton> rowButtons = new();
                int j = 0;
                while (j < 2 && usersEnumerator.MoveNext())
                {
                    TelegramUser user = usersEnumerator.Current;
                    rowButtons.Add(InlineKeyboardButton.WithCallbackData(
                        text: user.OsuUser.Username,
                        callbackData: GetRequireCallbackData(request.Id, nameof(request.ToUser), user.Id)));
                    j++;
                }
                if (rowButtons.Any())
                    keyboard.Add(rowButtons);
            }

            List<InlineKeyboardButton> buttons = new();
            if (page != 1)
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Back", $"{DATA}:{request.Id} A:{RequestAction.PageChange} P:{page - 1}"));
            else
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Back"));

            buttons.Add(InlineKeyboardButton.WithCallbackData($"Page {page}/{pagesCount}"));

            if (page != pagesCount)
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next ▶️", $"{DATA}:{request.Id} A:{RequestAction.PageChange} P:{page + 1}"));
            else
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next ▶️"));

            keyboard.Add(buttons);

            return new InlineKeyboardMarkup(keyboard);
        }

        private async Task<InlineKeyboardMarkup> CreateSnipeSelectMarkup(Request request)
        {
            IList<OsuScore>? scores = await _service.GetUserBeatmapAllScoresAsync(request.Beatmap.Id, request.FromUser.OsuUser.Id);
            if (scores is null)
            {
                request.RequirePass = true;
                return CreateRequireEditMarkup(request);
            }
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Выбери свое позорище:"),
                InlineKeyboardButton.WithCallbackData("❌ Cancel", $"{DATA}:{request.Id} A:{RequestAction.SnipeCancel}")
            });
            foreach (OsuScore score in scores)
            {
                keyboard.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"{ModsConverter.ToString(score.Mods)} Score: 💎{score.Score.Separate(".")} 🎯{score.Accuracy:0.00}% 🏆{score.MaxCombo}/{score.BeatmapAttributes.MaxCombo}x",
                        callbackData: $"{DATA}:{request.Id} A:{RequestAction.Snipe} M:{score.Mods} S:{score.Score} C:{score.MaxCombo} F:{score.Accuracy:0.00}")
                });
            }
            return new InlineKeyboardMarkup(keyboard);
        }

        private InlineKeyboardMarkup CreateRequireEditMarkup(Request request)
        {
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
            List<InlineKeyboardButton> rowButtons1 = new()
            {
                InlineKeyboardButton.WithCallbackData(
                        text: request.RequirePass ? "🍀 Pass 🟢" : "🍀 Pass 🔴",
                        callbackData: GetRequireCallbackData(request.Id, nameof(request.RequirePass), !request.RequirePass)),
                InlineKeyboardButton.WithCallbackData(
                        text: request.RequireFullCombo ? "🔥 FC 🟢" : "🔥 FC 🔴",
                        callbackData: GetRequireCallbackData(request.Id, nameof(request.RequireFullCombo), !request.RequireFullCombo)),
                InlineKeyboardButton.WithCallbackData(
                        text: request.RequireSnipe ? "🎯 Snipe 🟢" : "🎯 Snipe 🔴",
                        callbackData: $"{DATA}:{request.Id} A:{RequestAction.SnipeSelect}")
            };
            keyboard.Add(rowButtons1);

            List<InlineKeyboardButton> rowButtons2 = new();
            IEnumerable<Mod> mods = ModsConverter.ToMods(request.RequireMods);

            bool modHave = mods.Any(m => m.Name == NoMod.NAME);
            rowButtons2.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "NM ✅" : "NM ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - NoMod.NUMBER : request.RequireMods + NoMod.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModHardRock.NAME);
            rowButtons2.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "HR ✅" : "HR ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModHardRock.NUMBER : request.RequireMods + ModHardRock.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModDoubleTime.NAME);
            rowButtons2.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "DT ✅" : "DT ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModDoubleTime.NUMBER : request.RequireMods + ModDoubleTime.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModHidden.NAME);
            rowButtons2.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "HD ✅" : "HD ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModHidden.NUMBER : request.RequireMods + ModHidden.NUMBER))
            );

            keyboard.Add(rowButtons2);

            List<InlineKeyboardButton> rowButtons3 = new();

            modHave = mods.Any(m => m.Name == ModFlashlight.NAME);
            rowButtons3.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "FL ✅" : "FL ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModFlashlight.NUMBER : request.RequireMods + ModFlashlight.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModEasy.NAME);
            rowButtons3.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "EZ ✅" : "EZ ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModEasy.NUMBER : request.RequireMods + ModEasy.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModHalfTime.NAME);
            rowButtons3.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "HT ✅" : "HT ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModHalfTime.NUMBER : request.RequireMods + ModHalfTime.NUMBER))
            );

            rowButtons3.Add(InlineKeyboardButton.WithCallbackData(
                text: request.IsOnlyMods ? "Only mods" : "Any mods",
                callbackData: GetRequireCallbackData(request.Id, nameof(request.IsOnlyMods), !request.IsOnlyMods))
            );

            keyboard.Add(rowButtons3);

            List<InlineKeyboardButton> rowButtons4 = new()
            {
                InlineKeyboardButton.WithCallbackData(
                    text: "✅ Send",
                    callbackData: $"{DATA}:{request.Id} A:{RequestAction.Save}"),

                InlineKeyboardButton.WithCallbackData(
                    text: "❌ Cancel",
                    callbackData: $"{DATA}:{request.Id} A:{RequestAction.Cancel}")
            };
            keyboard.Add(rowButtons4);

            return new InlineKeyboardMarkup(keyboard);
        }

        private InlineKeyboardMarkup CreateSnipeRequireEditMarkup(Request request)
        {
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Ну и что из этого надо снайпнуть?"),
            });

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: request.RequireSnipeScore ? $"💎 Snipe score: {request.Score.Separate(".")} 🟢" : $"💎 Snipe score: {request.Score.Separate(".")} 🔴",
                    callbackData: GetSnipeRequireCallbackData(request.Id, nameof(request.RequireSnipeScore), !request.RequireSnipeScore)),

            });

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: request.RequireSnipeAccuracy ? $"🎯 Snipe accuracy: {request.Accuracy}% 🟢" : $"🎯 Snipe accuracy: {request.Accuracy}% 🔴",
                    callbackData: GetSnipeRequireCallbackData(request.Id, nameof(request.RequireSnipeAccuracy), !request.RequireSnipeAccuracy)),

            });

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: request.RequireSnipeCombo ? $"🏆 Snipe combo: {request.Combo.Separate(".")}x 🟢" : $"🏆 Snipe combo: {request.Combo.Separate(".")}x 🔴",
                    callbackData: GetSnipeRequireCallbackData(request.Id, nameof(request.RequireSnipeCombo), !request.RequireSnipeCombo)),

            });

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: "✅ Send",
                    callbackData: $"{DATA}:{request.Id} A:{RequestAction.Save}"),

                InlineKeyboardButton.WithCallbackData(
                    text: "❌ Cancel",
                    callbackData: $"{DATA}:{request.Id} A:{RequestAction.SnipeRequireCancel}")
            });

            return new InlineKeyboardMarkup(keyboard);
        }

        private async Task<InlineKeyboardMarkup> CreateMarkup(RequestAction action, Request request, string callbackQueryData, long chatId)
        {
            return action switch
            {
                RequestAction.Create => CreateUserSelectMarkup(request, callbackQueryData, chatId),
                RequestAction.PageChange => CreateUserSelectMarkup(request, callbackQueryData, chatId),
                RequestAction.RequireChange => CreateRequireEditMarkup(request),
                RequestAction.SnipeSelect => await CreateSnipeSelectMarkup(request),
                RequestAction.SnipeCancel => CreateRequireEditMarkup(request),
                RequestAction.Snipe => CreateSnipeRequireEditMarkup(request),
                RequestAction.SRC => CreateSnipeRequireEditMarkup(request),
                RequestAction.SnipeRequireCancel => await CreateSnipeSelectMarkup(request),
                _ => Extensions.ScoreKeyboardMarkup(request.Beatmap.Id, request.Beatmap.BeatmapsetId)
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

        public async Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
           if (callbackQuery.Data == null)
                return;

            if (callbackQuery.Message == null)
                return;

            string data = callbackQuery.Data;

            Match requestMatch = new Regex(@"R:(\d+) A:(\w+)").Match(data);
            if (!requestMatch.Success)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            int requestId = int.Parse(requestMatch.Groups[1].Value);
            RequestAction actionRequest = (RequestAction)Enum.Parse(typeof(RequestAction), requestMatch.Groups[2].Value);

            Request? request;
            if (actionRequest is RequestAction.Create)
            {
                OsuBeatmap? beatmap = await _service.GetBeatmapAsync(requestId);

                if (beatmap is null)
                    throw new Exception("При обработке запроса на реквест произошла ошибка");

                _database.Beatmaps.Upsert(beatmap);
                _database.Beatmapsets.Upsert(beatmap.Beatmapset);

                TelegramUser fromUser = _database.TelegramUsers.FindById(callbackQuery.From.Id);
                request = new Request(fromUser, beatmap);
            }
            else if (actionRequest is RequestAction.Save)
            {
                request = _database.Requests
                    .Include(r => r.FromUser)
                    .Include(r => r.FromUser.OsuUser)
                    .Include(r => r.ToUser)
                    .Include(r => r.ToUser.OsuUser)
                    .Include(r => r.Beatmap)
                    .Include(r => r.Beatmap.Beatmapset)
                    .FindById(requestId);
            }
            else
            {
                request = _database.Requests
                    .Include(u => u.FromUser)
                    .FindById(requestId);
            }

            if (request is null)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            if (callbackQuery.From.Id != request.FromUser.Id)
                return;

            if (actionRequest is RequestAction.RequireChange || actionRequest is RequestAction.SRC)
                request = ChangeRequireFromData(request, data);

            if (actionRequest is RequestAction.Snipe)
            {
                Match scoreMatch = new Regex(@"M:(\d+) S:(\d+) C:(\d+) F:(\d+\D\d+)").Match(data);
                if (!scoreMatch.Success)
                    throw new Exception("При обработке запроса на реквест произошла ошибка");

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

            if (actionRequest is RequestAction.SnipeCancel)
            {
                request.RequirePass = true;
                request.IsOnlyMods = false;
            }

            if (_actions.TryGetValue(actionRequest, out Action<Request>? action))
                action.Invoke(request);

            InlineKeyboardMarkup newReplyMarkup = await CreateMarkup(actionRequest, request, data, callbackQuery.Message.Chat.Id);

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

            if (actionRequest == RequestAction.Save)
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

                SKImage image = await ImageGenerator.Instance.CreateRequestCardAsync(request);

                await botClient.SendPhotoAsync(
                    chatId: callbackQuery.Message.Chat,
                    photo: new InputOnlineFile(image.Encode().AsStream()),
                    caption: $"@{fromMember.User.Username} создал реквест для @{toMember.User.Username} на карте {request.Beatmap.Url}",
                    replyMarkup: Extensions.RequestKeyboardMakrup(request.Beatmap.Id, request.Beatmap.BeatmapsetId),
                    cancellationToken: cancellationToken);
            }
        }
    }
}
