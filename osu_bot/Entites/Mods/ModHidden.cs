﻿using osu_bot.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public class ModHidden : Mod
    {
        public override int Number => 1 << 3;

        public override string Name => "HD";

        public override string Fullname => "Hidden";

        public override Image? Image => Resources.HD;
    }
}