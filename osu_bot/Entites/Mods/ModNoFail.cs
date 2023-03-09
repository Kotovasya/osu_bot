// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Assets;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModNoFail : Mod
    {
        public override int Number => 1 << 0;

        public override string Name => "NF";

        public override string Fullname => "No Fail";

        public override SKImage? Image => Resources.ModsManager.NF;
    }
}
