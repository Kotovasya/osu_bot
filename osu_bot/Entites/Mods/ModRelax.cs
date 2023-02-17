using osu_bot.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public class ModRelax : Mod
    {
        public override int Number => 1 << 7;

        public override string Name => "RL";

        public override string Fullname => "Relax";

        public override Image? Image => Resources.RL;
    }
}
