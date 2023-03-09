// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Assets;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    internal class ModsEasy : Mod, IApplicableMod
    {
        public override int Number => 1 << 1;

        public override string Name => "EZ";

        public override string Fullname => "Easy";

        public override SKImage? Image => Resources.ModsManager.EZ;

        public void ApplyToAttributes(BeatmapAttributes attributes)
        {
            double ratio = 0.5;
            attributes.CS = Math.Min(attributes.CS * ratio, 10.0);
            attributes.AR = Math.Min(attributes.AR * ratio, 10.0);
            attributes.OD = Math.Min(attributes.OD * ratio, 10.0);
            attributes.HP = Math.Min(attributes.HP * ratio, 10.0);
        }
    }
}
