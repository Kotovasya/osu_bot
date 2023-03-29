// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using SkiaSharp;
using Telegram.Bot.Types.Enums;

namespace osu_bot.Modules.Converters
{
    //[Flags]
    //public enum Mods
    //{
    //    ALL = int.MinValue
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
    //    NM = 1 << 30 ??
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
            new ModEasy(),
            new ModSpunOut(),
            new ModSuddenDeath(),
            new NoMod(),
            new AllMods(),
        };

        private static readonly Dictionary<int, Mod> s_intModsDictionary = new();
        private static readonly Dictionary<string, Mod> s_stringModsDictionary = new();

        static ModsConverter()
        {
            foreach (Mod mod in s_mods)
            {
                s_stringModsDictionary.Add(mod.Name, mod);
                s_intModsDictionary.Add(mod.Number, mod);
            }
        }

        public static Mod? ToMod(string str) => s_stringModsDictionary.ContainsKey(str) ? s_stringModsDictionary[str] : null;

        public static Mod? ToMod(int number) => s_intModsDictionary.ContainsKey(number) ? s_intModsDictionary[number] : null;

        public static IEnumerable<Mod>? ToMods(IEnumerable<string> mods)
        {
            HashSet<Mod> result = new();

            if (mods is null)
            {
                result.Add(s_intModsDictionary[AllMods.NUMBER]);
                return result;
            }

            if (!mods.Any())
            {
                result.Add(s_intModsDictionary[NoMod.NUMBER]);
                return result;
            }

            foreach (string modString in mods)
            {
                Mod? mod = ToMod(modString);
                if (mod is null)
                    throw new ModsArgumentException(modString);

                result.Add(mod);
            }
            return result;
        }

        public static IEnumerable<Mod> ToMods(int number)
        {
            HashSet<Mod> result = new();
            if (number == int.MinValue)
            {
                result.Add(s_intModsDictionary[AllMods.NUMBER]);
                return result;
            }

            int i = 0;
            while (number > 0)
            {
                if ((number & 1) == 1)
                {
                    int modNumber = (int)Math.Pow(2, i);
                    result.Add(s_intModsDictionary[modNumber]);
                }
                number >>= 1;
                i++;
            }
            return result;
        }

        public static string ToString(IEnumerable<Mod> mods)
        {
            StringBuilder sb = new();

            foreach (Mod mod in mods)
            {
                sb.Append($"{mod.Name},");
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        public static IEnumerable<string> ToStrings(IEnumerable<Mod> mods)
        {
            IEnumerable<string> stringsMods;
            if (mods.Any(m => m.Name == AllMods.NAME))
                stringsMods = Array.Empty<string>();
            else
                stringsMods = mods.Select(m => m.Name);
            return stringsMods;
        }

        public static IEnumerable<string> ToStrings(int mods)
        {
            return ToStrings(ToMods(mods));      
        }

        public static string ToString(int mods)
        {
            return ToString(ToMods(mods));
        }

        public static SKImage? ToImage(int mods)
        {
            return ToImage(ToMods(mods));
        }

        public static SKImage? ToImage(IEnumerable<Mod>? mods)
        {
            if (mods == null || !mods.Any() || mods.Any(m => m.Name == AllMods.NAME))
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

        public static int ToInt(IEnumerable<Mod>? mods)
        {
            if (mods is null)
            {
                return AllMods.NUMBER;
            }

            return mods.Sum(m => m.Number);
        }

        public static int ToInt(IEnumerable<string>? mods)
        {
            if (mods is null)
            {
                return AllMods.NUMBER;
            }

            return ToInt(ToMods(mods));
        }
    }
}
