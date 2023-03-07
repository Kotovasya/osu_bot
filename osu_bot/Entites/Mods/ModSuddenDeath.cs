﻿using osu_bot.Assets;
using osu_bot.Modules;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public class ModSuddenDeath : Mod
    {
        public override int Number => 1 << 5;

        public override string Name => "SD";

        public override string Fullname => "Sudden Death";

        public override SKImage? Image => Resources.ModsManager.SD;
    }
}
