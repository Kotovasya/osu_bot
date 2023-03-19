﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModPerfect : Mod
    {
        public const int NUMBER = 1 << 14;
        public const string NAME = "PF";
        public const string FULLNAME = "Perfect";

        public override int Number => NUMBER;

        public override string Name => NAME;

        public override string Fullname => FULLNAME;

        public override SKImage? Image => ResourcesManager.ModsManager.PF;
    }
}
