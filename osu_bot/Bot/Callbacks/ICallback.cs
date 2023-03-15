// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Callbacks
{
    public interface ICallback
    {
        public abstract string Data { get; }

        public abstract Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken);
    }
}
