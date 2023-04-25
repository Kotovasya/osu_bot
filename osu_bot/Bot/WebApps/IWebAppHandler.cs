// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Newtonsoft.Json.Linq;

namespace osu_bot.Bot.WebApps
{
    public interface IWebAppHandler
    {
        public abstract string AppName { get; }

        public abstract Task ActionAsync(ITelegramBotClient botClient, Message message, JObject data, CancellationToken cancellationToken);
    }
}
