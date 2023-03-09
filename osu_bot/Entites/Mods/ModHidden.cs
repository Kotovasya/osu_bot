﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Assets;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModHidden : Mod
    {
        public override int Number => 1 << 3;

        public override string Name => "HD";

        public override string Fullname => "Hidden";

        public override SKImage? Image => Resources.ModsManager.HD;
    }
}
