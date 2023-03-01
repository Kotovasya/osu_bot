using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
{
    public abstract class Mod : IEquatable<Mod>, IEqualityComparer<Mod>
    {
        public abstract int Number { get; }

        public abstract string Name { get; }

        public abstract string Fullname { get; }

        public abstract SKImage? Image { get; }

        public bool Equals(Mod? other)
        {
            return Number == other.Number;
        }

        public bool Equals(Mod? x, Mod? y)
        {
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] Mod obj)
        {
            return obj.Number;
        }
    }
}
