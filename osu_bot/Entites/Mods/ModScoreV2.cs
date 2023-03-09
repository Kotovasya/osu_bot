// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModScoreV2 : Mod
    {
        public override int Number => 1 << 29;

        public override string Name => "V2";

        public override string Fullname => "Score V2";

        public override SKImage? Image => throw new NotImplementedException();
    }
}
