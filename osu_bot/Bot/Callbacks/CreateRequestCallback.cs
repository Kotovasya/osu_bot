// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu_bot.Entites.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace osu_bot.Bot.Callbacks
{
    public class CreateRequestCallback : ICallback
    {
        public const string DATA = "Create request";

        public string Data => DATA;

        private readonly Dictionary<long, Request> Requests = new Dictionary<long, Request>()
        {

        };

        public async Task ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return;

            if (callbackQuery.Message == null)
                return;

            string data = callbackQuery.Data;

            Match scoreIdMatch = new Regex(@"scoreId(\d+)").Match(data);
            if (!scoreIdMatch.Success)
            {
                throw new Exception("При обработке запроса \"Создание реквеста\" произошла ошибка считывания ID скора");
            }
        }
    }
}
