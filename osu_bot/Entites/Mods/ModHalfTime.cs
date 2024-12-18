﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModHalfTime : Mod, IApplicableMod
    {
        public const int NUMBER = 1 << 8;
        public const string NAME = "HT";
        public const string FULLNAME = "Half Time";

        public override int Number => NUMBER;

        public override string Name => NAME;

        public override string Fullname => FULLNAME;

        public override SKImage? Image => ResourcesManager.ModsManager.HT;

        public void ApplyToAttributes(OsuBeatmapAttributes attributes)
        {
            attributes.AR = Math.Min((attributes.AR - 3.33) * 4 / 3, 9.0);
            attributes.OD = Math.Min((attributes.OD - 3.33) * 4 / 3, 9.0);
            attributes.HP = Math.Min((attributes.HP - 3.33) * 4 / 3, 9.0);
            attributes.HitLength = attributes.HitLength * 4 / 3;
            attributes.BPM = attributes.BPM * 3 / 4;
        }
    }
}
