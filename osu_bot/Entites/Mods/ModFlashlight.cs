// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModFlashlight : Mod
    {
        public override int Number => 1 << 10;

        public override string Name => "FL";

        public override string Fullname => "Flashlight";

        public override SKImage? Image => ResourcesManager.ModsManager.FL;
    }
}
