// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class AllMods : Mod
    {
        public const int NUMBER = int.MinValue;
        public const string NAME = "ALL";
        public const string FULLNAME = "ALL";

        public override int Number => NUMBER;

        public override string Name => NAME;

        public override string Fullname => FULLNAME;

        public override SKImage? Image => null;
    }
}
