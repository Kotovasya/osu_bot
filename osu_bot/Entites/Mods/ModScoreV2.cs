// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModScoreV2 : Mod
    {
        public const int NUMBER = 1 << 29;
        public const string NAME = "V2";
        public const string FULLNAME = "Score V2";

        public override int Number => NUMBER;

        public override string Name => NAME;

        public override string Fullname => FULLNAME;

        public override SKImage? Image => ResourcesManager.ModsManager.V2;
    }
}
