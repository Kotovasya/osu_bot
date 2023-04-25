// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Bot.WebApps;
using Telegram.Bot.Types;
using Telegram.Bot;
using Newtonsoft.Json.Linq;

namespace osu_bot.Bot
{
    public class WebAppsManager
    {
        private static readonly IWebAppHandler[] s_handlers = new[]
        {
            new WebReplaySettingsHandler(),
        };

        private readonly Dictionary<string, Func<ITelegramBotClient, Message, JObject, CancellationToken, Task>> _handlers = new();

        public WebAppsManager()
        {
            foreach (IWebAppHandler handler in s_handlers)
                _handlers.Add(handler.AppName, handler.ActionAsync);
        }

        public async Task HandlingAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.WebAppData is null)
                return;

            JObject obj = JObject.Parse(message.WebAppData.Data);

            string? key = obj["appName"]?.Value<string>();
            if (key is null)
                return;

            if (_handlers.ContainsKey(key))
            {
                obj.Remove("appName");
                await _handlers[key].Invoke(botClient, message, obj, cancellationToken);
            }
        }
    }
}
