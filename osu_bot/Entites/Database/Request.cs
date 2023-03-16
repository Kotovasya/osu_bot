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
        private bool _requireFullCombo;
        private float? _requireAccuracy;
        private int? _requireCombo;
        private float? _requireCompilation;

        public long Id { get; set; }

        public bool RequirePass
        {
            get => _requirePass;
            set => _requirePass = ScoreInfo.Rank != "F" && value;
        }

        public bool RequireFullCombo
        {
            get => _requireFullCombo;
            set => _requireFullCombo = ScoreInfo.IsFullCombo && value;
        }

        public float? RequireAccuracy
        {
            get => _requireAccuracy;
            set
            {
                if (value is null)
                    _requireAccuracy = null;
                else if (ScoreInfo.Accuracy >= value)
                    _requireAccuracy = value;
            }
        }

        public int? RequireCombo
        {
            get => _requireCombo;
            set
            {
                if (value is null)
                    _requireCombo = null;
                else if (ScoreInfo.MaxCombo >= value)
                    _requireCombo = value;
            }
        }

        public float? RequireCompletion
        {
            get => _requireCompilation;
            set
            {
                if (value is null)
                    _requireCompilation = null;
                else if (ScoreInfo.Compilation >= value)
                    _requireCompilation = value;
            }
        }

        public DateTime DateCreate { get; set; }
        public DateTime DateComplete { get; set; }

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

        public Request(long tempId)
            : this()
        {
            Id = tempId * -1;
        }

        public Request(TelegramUser fromUser, TelegramUser toUser, ScoreInfo score)
        {
            FromUser = fromUser;
            ToUser = toUser;
            ScoreInfo = score;
        }
    }
}
