// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Database
{
    public class BeatmapAttributesKey : IEquatable<BeatmapAttributesKey>
    {
        public long BeatmapId { get; set; }
        public int Mods { get; set; }

        public BeatmapAttributesKey() { }
        public BeatmapAttributesKey(long beatmapId, int mods)
        {
            BeatmapId = beatmapId;
            Mods = mods;
        }

        public bool Equals(BeatmapAttributesKey? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return BeatmapId == other.BeatmapId && Mods == other.Mods;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as BeatmapAttributesKey);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BeatmapId, Mods);
        }
    }
}
