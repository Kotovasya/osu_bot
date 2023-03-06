﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace osu_bot.Assets
{
    public static class Resources1
    {
        public static ModsManager ModsManager { get; } = new ModsManager();

        public static MapStatusManager MapStatusManager { get; } = new MapStatusManager();
    }
}
