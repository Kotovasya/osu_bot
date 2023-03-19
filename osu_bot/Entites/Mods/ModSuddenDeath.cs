// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModSuddenDeath : Mod
    {
        public const int NUMBER = 1 << 5;
        public const string NAME = "SD";
        public const string FULLNAME = "Sudden Death";

        public override int Number => NUMBER;

        public override string Name => NAME;

        public override string Fullname => FULLNAME;

        public override SKImage? Image => ResourcesManager.ModsManager.SD;
    }
}
