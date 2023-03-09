// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace osu_bot.Assets
{
    public class FontsManager : ResourceManager
    {
        protected override string ResourcesPath => "Assets\\Fonts";

        protected override string FileFormat => "ttf";

        protected override object ConvertFile(FileStream stream)
        {
            SKTypeface typeface = SKTypeface.FromStream(stream);
            return typeface;
        }

        [AllowNull]
        public SKTypeface Rubik { get; set; }

        [AllowNull]
        public SKTypeface SecularOne { get; set; }
    }
}
