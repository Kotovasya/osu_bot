// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json.Linq;
using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using osu_bot.Modules.OsuFiles;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Callbacks
{
    public enum ReplayCallbackAction
    {
        PressButton,
        Cancel,
        Send,
        SendExisting,
        SendAgain,
        PageChange,
    }

    public class ReplayCallback : ICallback
    {
        public const string DATA = "Replay";

        public string Data => DATA;

        private readonly OrdrAPI _api = OrdrAPI.Instance;
        private readonly OsuService _service = OsuService.Instance;
        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task<CallbackResult?> ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return null;

            if (callbackQuery.Message == null)
                return null;

            string data = callbackQuery.Data;

            Match replayMatch = new Regex(@"Replay:(\w+) A:(\w+)").Match(data);
            if (!replayMatch.Success)
                return new CallbackResult("При обработке запроса на реплей произошла ошибка", 500);

            string hash = replayMatch.Groups[1].Value;
            ReplayCallbackAction action = (ReplayCallbackAction)Enum.Parse(typeof(ReplayCallbackAction), replayMatch.Groups[2].Value);

            ReplayUpload? replay = _database.Replays.FindById(hash);

            if (replay is null && long.TryParse(hash, out long scoreId))
                replay = _database.Replays.FindOne(r => r.ScoreId == scoreId);

            if (action is ReplayCallbackAction.PressButton)
            {
                if (replay is not null && replay.Url is not null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat,
                        text: $"Реплей этого скора уже был загружен со скином {replay.Skin.Name}",
                        replyMarkup: MarkupGenerator.Instance.ReplayExistMarkup(hash),
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat,
                        text: $"Выбери один из пресетов, на котором нужно зарендерить видео",
                        replyMarkup: MarkupGenerator.Instance.ReplaySettingsChoose(callbackQuery.From.Id, 1, hash, DATA),
                        cancellationToken: cancellationToken);
                }
            }
            else if (action is ReplayCallbackAction.SendExisting)
            {
                if (replay is null)
                    throw new NotImplementedException();

                if (replay.MessageId is not null)
                {
                    await botClient.ForwardMessageAsync(
                        chatId: callbackQuery.Message.Chat,
                        fromChatId: callbackQuery.Message.Chat,
                        messageId: replay.MessageId.Value,
                        cancellationToken: cancellationToken);
                }
            }
            else if (action is ReplayCallbackAction.PageChange)
            {
                Match pageMatch = new Regex(@"P:(\d+)").Match(data);
                if (!pageMatch.Success)
                    return new CallbackResult("При обработке запроса на реплей произошла ошибка", 500);
                int page = int.Parse(pageMatch.Groups[1].Value);

                await botClient.EditMessageReplyMarkupAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    replyMarkup: MarkupGenerator.Instance.ReplaySettingsChoose(callbackQuery.From.Id, page, hash, DATA),
                    messageId: callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
            }
            else if (action is ReplayCallbackAction.Send || action is ReplayCallbackAction.SendAgain)
            {
                using MemoryStream replayData = await _service.GetReplayDataAsync(hash);
                if (replayData.Length == 0)
                    return new CallbackResult("Не удалось получить данные реплея из локальной БД и osu! API");

                TelegramUser user = _database.TelegramUsers
                    .Include(r => r.OsuUser)
                    .FindById(callbackQuery.From.Id);

                Match skinMatch = new Regex(@"settings:(\d+)").Match(data);
                if (!skinMatch.Success)
                    return new CallbackResult("При обработке запроса на реплей произошла ошибка", 500);

                int settingsId = int.Parse(skinMatch.Groups[1].Value);
                ReplaySettings settings = _database.ReplaySettings.FindById(settingsId);

                JObject jsonResponse = await _api.SendRenderAsync(user.OsuUser.Username, settings, replayData);
                if (jsonResponse["errorCode"].Value<int>() == 0)
                {
                    int renderId = jsonResponse["renderID"].Value<int>();
                    ReplayUpload upload = new(hash)
                    {
                        Skin = settings.Skin,
                        RenderId = renderId
                    };
                    _database.Replays.Insert(upload);
                }
                else
                    return new CallbackResult($"o!rdr API error: {jsonResponse["message"]}");
            }
            else if (action is ReplayCallbackAction.Cancel)
            {
                await botClient.DeleteMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
            }
            return CallbackResult.Success();
        }
    }
}
