using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public abstract class Mod
    {
        public abstract int Number { get; }

        public abstract string Name { get; }

        public abstract string Fullname { get; }

        public abstract Image? Image { get; }
    }
}
