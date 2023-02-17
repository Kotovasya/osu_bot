using osu_bot.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public class ModPerfect : Mod
    {
        public override int Number => 1 << 14;

        public override string Name => "PF";

        public override string Fullname => "Perfect";

        public override Image? Image => Resources.PF;
    }
}
