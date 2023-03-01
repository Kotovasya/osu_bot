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
    public class ModDoubleTime : Mod, IApplicableMod
    {
        public override int Number => 1 << 6;

        public override string Name => "DT";

        public override string Fullname => "Double Time";

        public override SKImage? Image => SKImage.FromEncodedData(Resources.DT.ToStream());

        public void ApplyToAttributes(BeatmapAttributes attributes)
        {
            attributes.AR = Math.Min((attributes.AR * 2 + 13) / 3, 11.0);
            attributes.OD = Math.Min((attributes.OD * 2 + 13) / 3 + 0.11, 11.11);
            attributes.HP = Math.Min((attributes.HP * 2 + 13) / 3, 11.0);
            attributes.Length = (int)Math.Round(attributes.Length * 0.66);
            attributes.BPM = (int)Math.Round(attributes.BPM * 1.5);
        }
    }
}
