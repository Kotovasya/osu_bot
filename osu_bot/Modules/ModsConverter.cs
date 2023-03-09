// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using SkiaSharp;

namespace osu_bot.Modules
{
    //[Flags]
    //public enum Mods
    //{
    //    NM = 0,
    //    NF = 1 << 0,    //No fail
    //    EZ = 1 << 1,    //Easy
    //    TD = 1 << 2,    //TouchDevice
    //    HD = 1 << 3,    //Hidden
    //    HR = 1 << 4,    //HardRock
    //    SD = 1 << 5,    //SuddenDeath
    //    DT = 1 << 6,    //DoubleTime
    //    RX = 1 << 7,    //Relax
    //    HT = 1 << 8,    //HalfTime
    //    NC = 1 << 9,    //Nightcore
    //    FL = 1 << 10,   //Flashlight
    //    AT = 1 << 11,   //Autoplay
    //    SO = 1 << 12,   //SpunOut
    //    AP = 1 << 13,   //Aotupilot
    //    PF = 1 << 14,   //Perfect
    //    K4 = 1 << 15,
    //    K5 = 1 << 16,
    //    K6 = 1 << 17,
    //    K7 = 1 << 18,
    //    K8 = 1 << 19,
    //    FI = 1 << 20,
    //    RN = 1 << 21,
    //    CN = 1 << 22,   //Cinema
    //    TP = 1 << 23,
    //    K9 = 1 << 24,
    //    K10 = 1 << 25,
    //    K1 = 1 << 26,
    //    K3 = 1 << 27,
    //    K2 = 1 << 28,
    //    V2 = 1 << 29,   //ScoreV2
    //    MR = 1 << 30,
    //    ALL = 1 << 31
    //}

    public static class ModsConverter
    {
        private static readonly List<Mod> s_mods = new()
        {
            new ModAuto(),
            new ModAutopilot(),
            new ModDoubleTime(),
            new ModFlashlight(),
            new ModHalfTime(),
            new ModHardRock(),
            new ModHidden(),
            new ModNightcore(),
            new ModNoFail(),
            new ModPerfect(),
            new ModRelax(),
            new ModScoreV2(),
            new ModsEasy(),
            new ModSpunOut(),
            new ModSuddenDeath(),
            new NoMod(),
        };

        private static readonly Dictionary<string, Mod> s_stringModsDictionary = new();

        static ModsConverter()
        {
            foreach (Mod mod in s_mods)
            {
                s_stringModsDictionary.Add(mod.Name, mod);
            }
        }

        public static Mod ToMod(string str) => s_stringModsDictionary.ContainsKey(str) ? s_stringModsDictionary[str] : throw new ModsArgumentException(str);

        public static IEnumerable<Mod> ToMods(IEnumerable<string> mods)
        {
            HashSet<Mod> result = new();

            if (!mods.Any())
            {
                _ = result.Add(ToMod("NM"));
                return result;
            }

            foreach (string modString in mods)
            {
                _ = result.Add(ToMod(modString));
            }
            return result;
        }

        public static string ToString(IEnumerable<Mod> mods)
        {
            StringBuilder sb = new();

            foreach (Mod mod in mods)
            {
                _ = sb.Append($"{mod.Name},");
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        public static SKImage? ToImage(IEnumerable<Mod>? mods)
        {
            if (mods == null || !mods.Any() || mods.Any(m => m.Name == "NM"))
            {
                return null;
            }

            using SKSurface surface = SKSurface.Create(new SKImageInfo(45 * mods.Count(), 32));
            SKCanvas canvas = surface.Canvas;
            int i = 0;
            foreach (Mod mod in mods)
            {
                canvas.DrawImage(mod.Image, 45 * i, 0);
                i++;
            }

            return surface.Snapshot();
        }
    }
}
