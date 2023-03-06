// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace osu_bot.Assets
{
    public class MapStatusManager : ResourceManager
    {
        protected override string ResourcesPath => "Assets\\Images\\Map Status";

        public SKImage? Approved { get; }

        public SKImage? Graveyard { get; }

        public SKImage? Loved { get; }

        public SKImage? Rating { get; }
    }
}
