using osu_bot.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    internal class ModsEasy : Mod, IApplicableMod
    {
        public override int Number => 1 << 1;

        public override string Name => "EZ";

        public override string Fullname => "Easy";

        public override Image? Image => Resources.EZ;

        public void ApplyToAttributes(BeatmapAttributes attributes)
        {
            double ratio = 0.5;
            attributes.CS *= ratio;
            attributes.AR *= ratio;
            attributes.OD *= ratio;
            attributes.HP *= ratio;
        }
    }
}
