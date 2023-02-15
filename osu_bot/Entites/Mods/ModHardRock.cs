using osu_bot.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public class ModHardRock : Mod, IApplicableMod
    {
        public override int Number => 1 << 4;

        public override string Name => "HR";

        public override string Fullname => "Hard Rock";

        public override Image? Image => Resources.HR;

        public void ApplyToAttributes(BeatmapAttributes attributes)
        {
            float ratio = 1.4f;
            attributes.CS *= 1.3f;
            attributes.AR *= ratio;
            attributes.OD *= ratio;
            attributes.HP *= ratio;
        }
    }
}
