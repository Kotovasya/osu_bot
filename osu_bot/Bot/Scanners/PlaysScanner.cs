// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using osu_bot.API;
using osu_bot.API.Handlers;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using osu_bot.Modules.Converters;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.Bot.Scanners
{
    public class PlaysScanner : Scanner
    {
        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;
        private readonly UserScoreQueryParameters _parameters = new(ScoreType.Recent, false)
        {
            LimitApi = 20,
            Limit = 20,
        };

        protected override TimeSpan Delay => TimeSpan.FromMinutes(10);

        protected override async Task ActionAsync()
        {
            IEnumerable<TelegramUser> users = _database.TelegramUsers.FindAll();

            foreach(TelegramUser user in users)
            {
                _parameters.UserId = user.OsuUser.Id;
                await _service.GetUserScoresAsync(_parameters);
            }
        }
    }
}
