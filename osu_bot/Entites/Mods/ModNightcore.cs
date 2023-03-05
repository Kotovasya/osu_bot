using osu_bot.Assets;
using osu_bot.Modules;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    internal class ModNightcore : Mod, IApplicableMod
    {
        public override int Number => 1 << 9;

        public override string Name => "NC";

        public override string Fullname => "Nightcore";

        public override SKImage? Image => Resources.NC.ToSKImage();

        public void ApplyToAttributes(BeatmapAttributes attributes)
        {
            attributes.AR = Math.Min((attributes.AR * 2 + 13) / 3, 11.0f);
            attributes.OD = Math.Min((attributes.OD * 2 + 13) / 3, 11.0f);
            attributes.HP = Math.Min((attributes.HP * 2 + 13) / 3, 11.0f);
            attributes.Length = (int)Math.Round(attributes.Length * 0.5f);
            attributes.BPM = (int)Math.Round(attributes.BPM * 1.5f);
        }
    }
}
