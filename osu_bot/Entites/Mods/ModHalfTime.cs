// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModHalfTime : Mod, IApplicableMod
    {
        public override int Number => 1 << 8;

        public override string Name => "HT";

        public override string Fullname => "Half Time";

        public override SKImage? Image => ResourcesManager.ModsManager.HT;

        public void ApplyToAttributes(OsuBeatmapAttributes attributes)
        {
            attributes.AR = Math.Min((attributes.AR - 3.33) * 4 / 3, 9.0);
            attributes.OD = Math.Min((attributes.OD - 3.33) * 4 / 3, 9.0);
            attributes.HP = Math.Min((attributes.HP - 3.33) * 4 / 3, 9.0);
            attributes.Length = attributes.Length * 4 / 3;
            attributes.BPM = attributes.BPM * 3 / 4;
        }
    }
}
