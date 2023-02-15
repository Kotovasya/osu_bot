using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public interface IApplicableMod
    {
        void ApplyToAttributes(BeatmapAttributes attributes);
    }
}
