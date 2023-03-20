// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class NoMod : Mod
    {
        public const int NUMBER = 1 << 30;
        public const string NAME = "NM";
        public const string FULLNAME = "NoMod";

        public override int Number => NUMBER;

        public override string Name => NAME;

        public override string Fullname => FULLNAME;

        public override SKImage? Image => null;
    }
}
