// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModRelax : Mod
    {
        public override int Number => 1 << 7;

        public override string Name => "RL";

        public override string Fullname => "Relax";

        public override SKImage? Image => ResourcesManager.ModsManager.RL;
    }
}
