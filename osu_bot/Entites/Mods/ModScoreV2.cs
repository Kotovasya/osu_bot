using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public class ModScoreV2 : Mod
    {
        public override int Number => 1 << 29;

        public override string Name => "V2";

        public override string Fullname => "Score V2";

        public override SKImage? Image => throw new NotImplementedException();
    }
}
