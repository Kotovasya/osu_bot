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
    public enum RequestActions
    {
        Create,
        Edit,
        Delete,
        Save,

    }

    public class RequestCallback : ICallback
    {
        public const string DATA = "Request";

        public string Data => DATA;

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        private readonly Dictionary<long, Request> _editableRequests = new();

        private readonly Dictionary<RequestActions, Action<Request>> _actions;

        public RequestCallback()
        {
            _actions = new()
            {
                { RequestActions.Create, (request) =>  },
                { RequestActions.Delete, (request) =>
                    {
                        if (_database.Requests.Delete(request.Id))
                            return null;
                    }
                },
            };
        }

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
