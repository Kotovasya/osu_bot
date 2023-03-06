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
    public class ModsManager : ResourceManager
    {
        protected override string ResourcesPath => "Assets\\Images\\Mods";

        public SKImage? AP { get; }

        public SKImage? AT { get; }

        public SKImage? DT { get; }

        public SKImage? EZ { get; }

        public SKImage? FL { get; }

        public SKImage? HD { get; }

        public SKImage? HR { get; }

        public SKImage? HT { get; }

        public SKImage? NC { get; }

        public SKImage? NF { get; }

        public SKImage? PF { get; }

        public SKImage? RL { get; }

        public SKImage? SD { get; }

        public SKImage? SO { get; }
    }
}
