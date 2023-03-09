﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Assets;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    internal class ModNightcore : Mod, IApplicableMod
    {
        public override int Number => 1 << 9;

        public override string Name => "NC";

        public override string Fullname => "Nightcore";

        public override SKImage? Image => Resources.ModsManager.NC;

        public void ApplyToAttributes(BeatmapAttributes attributes)
        {
            attributes.AR = Math.Min(((attributes.AR * 2) + 13) / 3, 11.0f);
            attributes.OD = Math.Min(((attributes.OD * 2) + 13) / 3, 11.0f);
            attributes.HP = Math.Min(((attributes.HP * 2) + 13) / 3, 11.0f);
            attributes.Length = (int)Math.Round(attributes.Length * 0.5f);
            attributes.BPM = (int)Math.Round(attributes.BPM * 1.5f);
        }
    }
}
