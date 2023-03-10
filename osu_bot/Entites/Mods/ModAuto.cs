// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModAuto : Mod
    {
        public override int Number => 1 << 11;

        public override string Name => "AT";

        public override string Fullname => "Auto";

        public override SKImage? Image => ResourcesManager.ModsManager.AT;
    }
}
