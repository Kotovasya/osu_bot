// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Bot.Callbacks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace osu_bot.Bot
{
    public class CallbacksManager
    {
        private static readonly ICallback[] s_callbacks =
{
            new HelpCallback(),
            new MapsCallback(),
            new MyScoreCallback(),
            new TopConferenceCallback(),
            new RequestCallback(),
        };

        private readonly Dictionary<string, Func<ITelegramBotClient, CallbackQuery, CancellationToken, Task>> _callbacks = new();

        public CallbacksManager()
        {
            foreach (ICallback callback in s_callbacks)
                _callbacks.Add(callback.Data, callback.ActionAsync);
        }

        public async Task HandlingAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data is { } data)
            {
                string? callbackData = _callbacks.Keys.FirstOrDefault(s => data.Contains(s));
                if (callbackData != null)
                {
                    await _callbacks[callbackData].Invoke(botClient, callbackQuery, cancellationToken);
                }
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
