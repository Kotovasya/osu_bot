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
            double ratio = 1.4;
            attributes.CS = Math.Min(attributes.CS * 1.3, 10.0);
            attributes.AR = Math.Min(attributes.AR * ratio, 10.0);
            attributes.OD = Math.Min(attributes.OD * ratio, 10.0);
            attributes.HP = Math.Min(attributes.HP * ratio, 10.0);
        }
    }
}
