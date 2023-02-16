using osu_bot.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public class ModNoFail : Mod
    {
        public override int Number => 1 << 0;

        public override string Name => "NF";

        public override string Fullname => "No Fail";

        public override Image? Image => Resources.NF;
    }
}
