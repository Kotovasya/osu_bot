// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModDoubleTime : Mod, IApplicableMod
    {
        public override int Number => 1 << 6;

        public override string Name => "DT";

        public override string Fullname => "Double Time";

        public override SKImage? Image => ResourcesManager.ModsManager.DT;

        public void ApplyToAttributes(OsuBeatmapAttributes attributes)
        {
            attributes.AR = Math.Min(((attributes.AR * 2) + 13) / 3, 11.0);
            attributes.OD = Math.Min((((attributes.OD * 2) + 13) / 3) + 0.11, 11.11);
            attributes.HP = Math.Min(((attributes.HP * 2) + 13) / 3, 11.0);
            attributes.Length = (int)Math.Round(attributes.Length * 0.66);
            attributes.BPM = (int)Math.Round(attributes.BPM * 1.5);
        }
    }
}
