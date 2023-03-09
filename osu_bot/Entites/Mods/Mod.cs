// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using SkiaSharp;

namespace osu_bot.Entites.Mods
{
    public abstract class Mod : IEquatable<Mod>, IEqualityComparer<Mod>
    {
        public abstract int Number { get; }

        public abstract string Name { get; }

        public abstract string Fullname { get; }

        public abstract SKImage? Image { get; }

        public bool Equals(Mod? other) => other != null && Number == other.Number;

        public bool Equals(Mod? x, Mod? y) => x != null && x.Equals(y);

        public int GetHashCode([DisallowNull] Mod obj) => obj.Number;
    }
}
