﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModHardRock : Mod, IApplicableMod
    {
        public const int NUMBER = 1 << 4;
        public const string NAME = "HR";
        public const string FULLNAME = "HardRock";

        public override int Number => NUMBER;

        public override string Name => NAME;

        public override string Fullname => FULLNAME;

        public override SKImage? Image => ResourcesManager.ModsManager.HR;

        public void ApplyToAttributes(OsuBeatmapAttributes attributes)
        {
            double ratio = 1.4;
            attributes.CS = Math.Min(attributes.CS * 1.3, 10.0);
            attributes.AR = Math.Min(attributes.AR * ratio, 10.0);
            attributes.OD = Math.Min(attributes.OD * ratio, 10.0);
            attributes.HP = Math.Min(attributes.HP * ratio, 10.0);
        }
    }
}
