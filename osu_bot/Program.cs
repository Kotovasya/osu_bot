﻿using Newtonsoft.Json;
using osu_bot.Bot;
using osu_bot.Entites;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using osu_bot.Modules;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Formats.Asn1.AsnWriter;
using User = osu_bot.Entites.User;

namespace osu_bot
{
    [SupportedOSPlatform("windows")]
    internal class Program
    {
        static async Task Main(string[] args)
        {
            BotHandle bot = new BotHandle();
            await bot.Run();
        }
    }
}