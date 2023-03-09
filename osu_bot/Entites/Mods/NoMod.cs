// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class NoMod : Mod
    {
        public override int Number => 0;

        public override string Name => "NM";

        public override string Fullname => "NoMod";

        public override SKImage? Image => null;
    }
}
