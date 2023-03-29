// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using osu_bot.Entites.Mods;
using osu_bot.Modules.Converters;
using Telegram.Bot.Requests.Abstractions;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.Entites.Database
{
    public class Request
    {
        private bool _requirePass;
        private bool _requireFullCombo;
        private bool _requireSnipe;

        private bool _requireSnipeScore;
        private bool _requireSnipeAcc;
        private bool _requireSnipeCombo;

        private int _requireMods;
        private bool _isOnlyMods;

        public int Id { get; set; }

        public bool RequirePass
        {
            get => _requirePass;
            set
            {
                if (value)
                {
                    _requirePass = value;
                    _requireSnipe = false;
                    _requireFullCombo = false;
                }
                CheckRequrieTaskValid();
            }
        }

        public bool RequireFullCombo
        {
            get => _requireFullCombo;
            set
            {
                if (value)
                {
                    _requireFullCombo = value;
                    _requireSnipe = false;
                    _requirePass = false;
                }
                CheckRequrieTaskValid();
            }
        }

        public bool RequireSnipe
        {
            get => _requireSnipe;
            set
            {
                if (value)
                {
                    _requireSnipe = value;
                    _requireFullCombo = false;
                    _requirePass = false;
                }
                CheckRequrieTaskValid();
            }
        }

        public bool IsOnlyMods
        {
            get => _isOnlyMods;
            set
            {
                if (value)
                    RequireMods = NoMod.NUMBER;
                else
                    RequireMods = NoMod.NUMBER + ModHidden.NUMBER + ModHardRock.NUMBER + ModDoubleTime.NUMBER + ModFlashlight.NUMBER;
                _isOnlyMods = value;
            }
        }

        public int RequireMods
        {
            get => _requireMods;
            set
            {
                IEnumerable<Mod> mods = ModsConverter.ToMods(value);
                if (IsOnlyMods)
                {
                    Mod? newMod = null;
                    if (value >= _requireMods)
                        newMod = ModsConverter.ToMod(value - _requireMods);
                    if (newMod is not null)
                    {
                        mods = newMod.Name switch
                        {
                            NoMod.NAME => mods.Where(m => m.Name == NoMod.NAME),
                            ModHardRock.NAME => mods.Where(m => m.Name != ModEasy.NAME),
                            ModEasy.NAME => mods.Where(m => m.Name != ModHardRock.NAME),
                            ModDoubleTime.NAME => mods.Where(m => m.Name != ModHalfTime.NAME),
                            ModHalfTime.NAME => mods.Where(m => m.Name != ModDoubleTime.NAME),
                            _ => mods,
                        };
                        if (newMod.Name != NoMod.NAME)
                            mods = mods.Where(m => m.Name != NoMod.NAME);
                    }
                }
                _requireMods = ModsConverter.ToInt(mods);
            }
        }

        public bool RequireSnipeScore
        {
            get => _requireSnipeScore;
            set
            {
                if (value)
                {
                    _requireSnipeScore = value;
                    _requireSnipeAcc = false;
                    _requireSnipeCombo = false;
                }
                CheckRequrieTaskValid();
            }
        }

        public bool RequireSnipeAcc
        {
            get => _requireSnipeAcc;
            set
            {
                if (value)
                {
                    _requireSnipeAcc = value;
                    _requireSnipeScore = false;
                    _requireSnipeCombo = false;
                }
                CheckRequrieTaskValid();
            }
        }

        public bool RequireSnipeCombo
        {
            get => _requireSnipeCombo;
            set
            {
                if (value)
                {
                    _requireSnipeCombo = value;
                    _requireSnipeScore = false;
                    _requireSnipeAcc = false;
                }
                CheckRequrieTaskValid();
            }
        }

        public int Score { get; set; }

        public float Accuracy { get; set; }

        public int Combo { get; set; }

        public bool IsTemporary { get; set; }

        public DateTime DateCreate { get; set; }
        public DateTime DateComplete { get; set; }

        public bool IsComplete { get; set; }

        [BsonRef]
        public TelegramUser FromUser { get; set; }
        [BsonRef]
        public TelegramUser ToUser { get; set; }

        [BsonRef]
        public OsuBeatmap Beatmap { get; set; }

        [BsonRef]
        public OsuBeatmapAttributes BeatmapAttributes { get; set; }

        public Request()
        {

        }

        public Request(TelegramUser fromUser, TelegramUser toUser)
        {
            FromUser = fromUser;
            ToUser = toUser;
            DateCreate = DateTime.Now;
            IsTemporary = true;
            _requireMods = NoMod.NUMBER + ModHidden.NUMBER + ModHardRock.NUMBER + ModDoubleTime.NUMBER + ModFlashlight.NUMBER;
            _requirePass = true;
            _requireSnipeScore = true;
        }

        public Request(TelegramUser requestOwner, OsuBeatmap beatmap)
            : this(requestOwner, new TelegramUser())
        {
            Beatmap = beatmap;
        }

        private void CheckRequrieTaskValid()
        {
            if (!_requirePass && !_requireFullCombo && !_requireSnipe)
                _requirePass = true;

            if (!_requireSnipeScore && !_requireSnipeAcc && !_requireSnipeCombo)
                _requireSnipeScore = true;
        }
    }
}
