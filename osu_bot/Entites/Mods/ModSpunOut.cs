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
    public class ModSpunOut : Mod
    {
        public override int Number => 1 << 12;

        public override string Name => "SO";

        public override string Fullname => "Spun Out";

        public override SKImage? Image => SKImage.FromEncodedData(Resources.SO.ToStream());
    }
}
