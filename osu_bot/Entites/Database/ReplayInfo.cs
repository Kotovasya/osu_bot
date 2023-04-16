// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace osu_bot.Entites.Database
{
    public class ReplayInfo
    {
        [BsonId]
        public string ReplayHash { get; set; }

        public long? ScoreId { get; set; }

        public string? TelegramFileId { get; set; }

        public string? ReplayUrl { get; set; }

        public ReplayInfo(string hash)
        {
            ReplayHash = hash;
        }

        public ReplayInfo()
        {

        }
    }
}
