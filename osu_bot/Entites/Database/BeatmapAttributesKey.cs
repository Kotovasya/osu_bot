﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Database
{
    public class BeatmapAttributesKey
    {
        public int BeatmapId { get; set; }
        public int Mods { get; set; }

        public BeatmapAttributesKey() { }
        public BeatmapAttributesKey(int beatmapId, int mods)
        {
            BeatmapId = beatmapId;
            Mods = mods;
        }
    }
}
