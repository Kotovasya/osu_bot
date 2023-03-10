// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModAutopilot : Mod
    {
        public override int Number => 1 << 13;

        public override string Name => "AP";

        public override string Fullname => "Autopilot";

        public override SKImage? Image => ResourcesManager.ModsManager.AP;
    }
}
