﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Exceptions
{
    public class UserScoreOnBeatmapNotFoundException : Exception
    {
        public string Username { get; set; }
        public long BeatmapId { get; set; }

        public UserScoreOnBeatmapNotFoundException(string username, long beatmapId)
        {
            Username = username;
            BeatmapId = beatmapId;
        }

        public override string Message =>
            $"У пользователя {Username} отсутствуют скоры на карте {BeatmapId}";
    }
}
