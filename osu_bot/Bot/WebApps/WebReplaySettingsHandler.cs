// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu_bot.Entites.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.WebApps
{
    public class WebReplaySettingsHandler : IWebAppHandler
    {
        public string AppName => "ReplaySettings";

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, JObject data, CancellationToken cancellationToken)
        {
            if (message.WebAppData is null)
                return;

            ReplaySettings? settings = JsonConvert.DeserializeObject<ReplaySettings>(message.WebAppData.Data);
            if (settings is null)
                return;

            bool result = _database.ReplaySettings.Upsert(settings);
            string text = result ? "Пресет успешно сохранен" : "Не удалось сохранить пресет";
            await botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: text,
                cancellationToken: cancellationToken);
        }
    }
}
