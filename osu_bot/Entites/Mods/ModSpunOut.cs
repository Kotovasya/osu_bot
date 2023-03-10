// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModSpunOut : Mod
    {
        public override int Number => 1 << 12;

        public override string Name => "SO";

        public override string Fullname => "Spun Out";

        public override SKImage? Image => ResourcesManager.ModsManager.SO;
    }
}
