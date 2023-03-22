// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Entites.Mods;
using osu_bot.Modules.Converters;
using Telegram.Bot.Requests.Abstractions;

namespace osu_bot.Entites.Database
{
    public class Request
    {
        private bool _requirePass;
        private bool _requireFullcombo;
        private bool _requireSnipe;
        private int _requireMods;

        public long Id { get; set; }

        public long BeatmapId { get; set; }

        public long BeatmapsetId { get; set; }

        public bool RequirePass
        {
            get => _requirePass;
            set
            {          
                if (value)
                {
                    _requirePass = value;
                    _requireSnipe = false;
                    _requireFullcombo = false;
                }
            }
        }

        public bool RequireFullCombo
        {
            get => _requireFullcombo;
            set
            {              
                if (value)
                {
                    _requireFullcombo = value;
                    _requireSnipe = false;
                    _requirePass = false;
                }
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
                    _requireFullcombo = false;
                    _requirePass = false;
                }
            }
        }

        public int RequireMods
        {
            get => _requireMods;
            set
            {
                IEnumerable<Mod> mods = ModsConverter.ToMods(value);
                if (IsAllMods)
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

        public bool IsAllMods { get; set; }

        public bool IsTemporary { get; set; }

        public DateTime DateCreate { get; set; }
        public DateTime DateComplete { get; set; }

        public bool IsComplete { get; set; }

        public TelegramUser FromUser { get; set; }
        public TelegramUser ToUser { get; set; }

        public Request()
        {

        }

        public Request(TelegramUser fromUser, TelegramUser toUser)
        {
            FromUser = fromUser;
            ToUser = toUser;
            DateCreate = DateTime.Now;
            IsTemporary = true;
            _requireMods = NoMod.NUMBER;
            _requirePass = true;
        }

        public Request(TelegramUser requestOwner, long beatmapId)
            : this(requestOwner, new TelegramUser())
        {
            BeatmapId = beatmapId;
        }
    }
}
