// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModSuddenDeath : Mod
    {
        public override int Number => 1 << 5;

        public override string Name => "SD";

        public override string Fullname => "Sudden Death";

        public override SKImage? Image => ResourcesManager.ModsManager.SD;
    }
}
