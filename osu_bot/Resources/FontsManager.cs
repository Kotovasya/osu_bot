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
    public class FontsManager : ResourceManager<SKTypeface>
    {
        protected override string ResourcesPath => @"Assets/Fonts";

        protected override string FileFormat => "ttf";

        protected override SKTypeface ConvertFile(FileStream stream) => SKTypeface.FromStream(stream);

        [AllowNull]
        public SKTypeface SecularOne { get; private set; }

        [AllowNull]
        public SKTypeface Rubik { get; private set; }

        [AllowNull]
        public SKTypeface RubikLight { get; private set; }

        [AllowNull]
        public SKTypeface RubikMedium { get; private set; }
    }
}
