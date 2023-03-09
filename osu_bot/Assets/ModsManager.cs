// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using SkiaSharp;

namespace osu_bot.Assets
{
    public class ModsManager : ResourceManager
    {
        protected override string ResourcesPath => "Assets\\Images\\Mods";

        protected override string FileFormat => "png";

        protected override object ConvertFile(FileStream stream)
        {
            return SKImage.FromEncodedData(stream);
        }

        [AllowNull]
        public SKImage AP { get; private set; }

        [AllowNull]
        public SKImage AT { get; private set; }

        [AllowNull]
        public SKImage DT { get; private set; }

        [AllowNull]
        public SKImage EZ { get; private set; }

        [AllowNull]
        public SKImage FL { get; private set; }

        [AllowNull]
        public SKImage HD { get; private set; }

        [AllowNull]
        public SKImage HR { get; private set; }

        [AllowNull]
        public SKImage HT { get; private set; }

        [AllowNull]
        public SKImage NC { get; private set; }

        [AllowNull]
        public SKImage NF { get; private set; }

        [AllowNull]
        public SKImage PF { get; private set; }

        [AllowNull]
        public SKImage RL { get; private set; }

        [AllowNull]
        public SKImage SD { get; private set; }

        [AllowNull]
        public SKImage SO { get; private set; }
    }
}
