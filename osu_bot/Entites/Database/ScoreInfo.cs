﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using osu_bot.Entites.Mods;
using osu_bot.Modules.Converters;

namespace osu_bot.Entites.Database
{
    public class ScoreInfo
    {
        public long Id { get; set; }
        public int Score { get; set; }
        public float Accuracy { get; set; }
        public DateTime Date { get; set; }
        public int MaxCombo { get; set; }
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public int Count300 { get; set; }
        public int CountMisses { get; set; }
        public string Rank { get; set; }
        public long BeatmapId { get; set; }
        public int Mods { get; set; }
        public float Completion { get; set; }
        public bool IsFullCombo { get; set; }

        [BsonRef("TelegramUser")]
        public TelegramUser User { get; set; }

        public ScoreInfo()
        {
            User = new TelegramUser();
        }

        public ScoreInfo(OsuScoreInfo score)
        {
            Id = score.Id;
            Score = score.Score;
            Accuracy = score.Accuracy;
            Date = score.Date;
            MaxCombo = score.MaxCombo;
            Count50 = score.Count50;
            Count100 = score.Count100;
            Count300 = score.Count300;
            CountMisses = score.CountMisses;
            Rank = score.Rank;
            BeatmapId = score.Beatmap.Id;
            Completion = score.Compilation;
            IsFullCombo = score.IsFullCombo;
            Mods = ModsConverter.ToInt(score.Mods);
        }
    }
}
