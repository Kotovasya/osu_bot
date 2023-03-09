// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Assets;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModPerfect : Mod
    {
        public override int Number => 1 << 14;

        public override string Name => "PF";

        public override string Fullname => "Perfect";

        public override SKImage? Image => Resources.ModsManager.PF;
    }
}
