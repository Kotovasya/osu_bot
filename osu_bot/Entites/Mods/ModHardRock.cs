// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Assets;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public class ModHardRock : Mod, IApplicableMod
    {
        public override int Number => 1 << 4;

        public override string Name => "HR";

        public override string Fullname => "Hard Rock";

        public override SKImage? Image => Resources.ModsManager.HR;

        public void ApplyToAttributes(BeatmapAttributes attributes)
        {
            double ratio = 1.4;
            attributes.CS = Math.Min(attributes.CS * 1.3, 10.0);
            attributes.AR = Math.Min(attributes.AR * ratio, 10.0);
            attributes.OD = Math.Min(attributes.OD * ratio, 10.0);
            attributes.HP = Math.Min(attributes.HP * ratio, 10.0);
        }
    }
}
