// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Exceptions
{
    public class BeatmapNotFoundException : Exception
    {
        public long BeatmapId { get; set; }

        public override string Message => $"Карта с ID {BeatmapId} не найдена";

        public BeatmapNotFoundException(long beatmapId) => BeatmapId = beatmapId;
    }
}
