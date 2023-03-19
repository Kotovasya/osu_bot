// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Entites.Mods;

namespace osu_bot.Entites.Database
{
    public class Request
    {
        public long Id { get; set; }

        public long BeatmapId { get; set; }

        public bool RequirePass { get; set; }
        public bool RequireFullCombo { get; set; }
        public bool RequireSnipe { get; set; }
        public int RequireMods { get; set; }
        public bool IsAllMods { get; set; }

        public bool IsTemporary { get; set; }

        public DateTime DateCreate { get; set; }
        public DateTime DateComplete { get; set; }

        public bool IsComplete { get; set; }

        public TelegramUser FromUser { get; set; }
        public TelegramUser ToUser { get; set; }

        public Request()
            : this(new TelegramUser(), new TelegramUser())
        {

        }

        public Request(TelegramUser fromUser, TelegramUser toUser)
        {
            FromUser = fromUser;
            ToUser = toUser;
            DateCreate = DateTime.Now;
            IsTemporary = true;
        }

        public Request(TelegramUser requestOwner)
            : this(requestOwner, new TelegramUser())
        {

        }
    }
}
