﻿using osu_bot.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public class ModAuto : Mod
    {
        public override int Number => 1 << 11;

        public override string Name => "AT";

        public override string Fullname => "Auto";

        public override Image? Image => Resources.AT;
    }
}