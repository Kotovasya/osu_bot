using osu_bot.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public class ModFlashlight : Mod
    {
        public override int Number => 1 << 10;

        public override string Name => "FL";

        public override string Fullname => "Flashlight";

        public override Image? Image => Resources.FL;
    }
}
