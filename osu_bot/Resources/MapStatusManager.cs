﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using SkiaSharp;

namespace osu_bot.Resources
{
    public class MapStatusManager : ResourceManager<SKImage>
    {
        protected override string ResourcesPath => @"Assets/Images/Map Status";

        protected override string FileFormat => "png";

        protected override SKImage ConvertFile(FileStream stream) => SKImage.FromEncodedData(stream);

        [AllowNull]
        public SKImage Approved { get; private set; }

        [AllowNull]
        public SKImage Graveyard { get; private set; }

        [AllowNull]
        public SKImage Loved { get; private set; }

        [AllowNull]
        public SKImage Rating { get; private set; }
    }
}
