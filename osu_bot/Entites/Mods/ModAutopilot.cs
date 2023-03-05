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
    public class ModAutopilot : Mod
    {
        public override int Number => 1 << 13;

        public override string Name => "AP";

        public override string Fullname => "Autopilot";

        public override SKImage? Image => Resources.AP.ToSKImage();
    }
}
