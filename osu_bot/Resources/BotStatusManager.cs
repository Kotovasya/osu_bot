// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace osu_bot.Resources
{
    public class BotStatusManager : ResourceManager<SKImage>
    {
        protected override string ResourcesPath => @"Assets/Images/Bot Status";

        protected override string FileFormat => "png";

        protected override SKImage ConvertFile(FileStream stream) => SKImage.FromEncodedData(stream);

        [AllowNull]
        public SKImage Online { get; private set; }

        [AllowNull]
        public SKImage Offline { get; private set; }
    }
}
