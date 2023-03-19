// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Resources;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    internal class ModNightcore : Mod, IApplicableMod
    {
        public const int NUMBER = 1 << 9;
        public const string NAME = "NC";
        public const string FULLNAME = "Nightcore";

        public override int Number => NUMBER;

        public override string Name => NAME;

        public override string Fullname => FULLNAME;

        public override SKImage? Image => ResourcesManager.ModsManager.NC;

        public void ApplyToAttributes(OsuBeatmapAttributes attributes)
        {
            attributes.AR = Math.Min(((attributes.AR * 2) + 13) / 3, 11.0f);
            attributes.OD = Math.Min(((attributes.OD * 2) + 13) / 3, 11.0f);
            attributes.HP = Math.Min(((attributes.HP * 2) + 13) / 3, 11.0f);
            attributes.Length = (int)Math.Round(attributes.Length * 0.5f);
            attributes.BPM = (int)Math.Round(attributes.BPM * 1.5f);
        }
    }
}
