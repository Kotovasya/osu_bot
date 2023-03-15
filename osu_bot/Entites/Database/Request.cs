// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace osu_bot.Entites.Database
{
    public class Request
    {
        private bool _requirePass;
        private bool _requireFullcombo;


        public long Id { get; set; }

        public bool RequirePass { get; set; }
        public bool RequireFullCombo { get; set; }
        public float? RequireAccuracy { get; set; }
        public int? RequireCombo { get; set; }
        public int? RequirePP { get; set; }
        public float? RequireCompletion { get; set; }

        public DateTime DateCreate { get; set; }
        public DateTime DateComplete { get; set; }

        public long BeatmapId { get; set; }
        public bool IsComplete { get; set; }

        public TelegramUser FromUser { get; set; }
        public TelegramUser ToUser { get; set; }
        public ScoreInfo ScoreInfo { get; set; }

        public Request()
        {
            FromUser = new TelegramUser();
            ToUser = new TelegramUser();
            ScoreInfo = new ScoreInfo();
        }

        public Request(TelegramUser fromUser, TelegramUser toUser, ScoreInfo score)
        {
            FromUser = fromUser;
            ToUser = toUser;
            ScoreInfo = score;
            BeatmapId = score.BeatmapId;
        }
    }
}
