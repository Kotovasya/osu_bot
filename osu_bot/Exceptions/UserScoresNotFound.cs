// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.API;
using osu_bot.Entites.Mods;
using osu_bot.Modules.Converters;
using Telegram.Bot.Types;

namespace osu_bot.Exceptions
{
    public class UserScoresNotFound : Exception
    {
        public string Username { get; set; }

        public ScoreType? Type { get; set; }

        public long? BeatmapId { get; set; }

        public int Mods { get; set; }

        public override string Message => FormatString();

        public UserScoresNotFound(string username, long beatmapId, int mod = AllMods.NUMBER)
        {
            Username = username;
            Type = null;
            Mods = mod;
            BeatmapId = beatmapId;
        }

        public UserScoresNotFound(string username, ScoreType? type = null, int mods = AllMods.NUMBER)
        {
            Username = username;
            Type = type;
            Mods = mods;
            BeatmapId = null;
        }

        private string FormatString()
        {
            if (BeatmapId is not null)
                return $"У пользователя {Username} отсутствуют скоры{ConvertMod()} на карте {BeatmapId}";

            return Type switch
            {
                ScoreType.Best => $"У пользователя {Username} отсутствуют топ скоры{ConvertMod()}",
                ScoreType.Recent => $"У пользователя {Username} отсутствуют скоры{ConvertMod()} за последние 24 часа",
                _ => $"У пользователя {Username} отсутствуют скоры"
            };
        }

        private string ConvertMod()
        {
            return Mods switch
            {
                AllMods.NUMBER => "",
                _ => $" с модами {ModsConverter.ToString(Mods)}"
            };
        }
    }
}
