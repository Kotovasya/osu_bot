// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Callbacks
{
    public enum ReplaySettingsCallbackAction
    {
        Create,
        Update,
        Delete,
        Select,
        Cancel,
        Hide,
        PageChange,
        Save
    }

    public class ReplaySettingsCallback : ICallback
    {
        public const string DATA = "RS";

        public string Data => DATA;

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task<CallbackResult?> ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return null;

            if (callbackQuery.Message == null)
                return null;

            string data = callbackQuery.Data;

            Match replayMatch = new Regex(@"RS:(\d+) A:(\w+)").Match(data);
            if (!replayMatch.Success)
                return new CallbackResult("При обработке запроса на реплей произошла ошибка", 500);

            int id = int.Parse(replayMatch.Groups[1].Value);
            ReplaySettingsCallbackAction action = (ReplaySettingsCallbackAction)Enum.Parse(typeof(ReplaySettingsCallbackAction), replayMatch.Groups[2].Value);

            if (action is ReplaySettingsCallbackAction.Select)
            {
                await botClient.EditMessageReplyMarkupAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    replyMarkup: MarkupGenerator.Instance.ReplaySettingsSelect(id),
                    messageId: callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
            }
            else if (action is ReplaySettingsCallbackAction.Cancel)
            {
                await botClient.EditMessageReplyMarkupAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    replyMarkup: MarkupGenerator.Instance.ReplaySettingsList(callbackQuery.From.Id, 1),
                    messageId: callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
            }
            else if (action is ReplaySettingsCallbackAction.Hide)
            {
                await botClient.DeleteMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
            }
            else if (action is ReplaySettingsCallbackAction.Create)
            {
                WebAppInfo webApp = new() { Url = new ReplaySettings().GetWebPageString() };
                ReplyKeyboardMarkup markup = new(new KeyboardButton[]
                {
                    KeyboardButton.WithWebApp("🆕 Создать пресет", webApp),
                    new KeyboardButton("❌ Отмена"),
                })
                {
                    OneTimeKeyboard = true,
                    ResizeKeyboard = true,
                };
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Для создания пресета, нажмите кнопку внизу экрана",
                    replyMarkup: markup,
                    cancellationToken: cancellationToken);
            }
            else if (action is ReplaySettingsCallbackAction.Delete)
            {
                try
                {
                    _database.ReplaySettings.Delete(id);
                    await botClient.EditMessageReplyMarkupAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        replyMarkup: MarkupGenerator.Instance.ReplaySettingsList(callbackQuery.From.Id, 1),
                        messageId: callbackQuery.Message.MessageId,
                        cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("message is not modified"))
                        throw;
                }
            }
            return CallbackResult.Success();
        }
    }
}
