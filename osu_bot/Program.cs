// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using osu_bot.API;
using osu_bot.API.Queries;
using osu_bot.Bot;
using osu_bot.Entites;
using osu_bot.Modules;
using SkiaSharp;

namespace osu_bot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            BotHandle bot = new();
            await bot.Run();
        }
    }
}
