using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Assets;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;

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

    [SupportedOSPlatform("windows")]
    public static class ModsConverter
    {
        private static readonly List<Mod> Mods = new()
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

        private static readonly Dictionary<string, Mod> StringModsDictionary = new();

        static ModsConverter()
        {
            foreach (var mod in Mods)
                StringModsDictionary.Add(mod.Name, mod);
        }

        public static Mod ToMod(string str)
        {
            if (StringModsDictionary.ContainsKey(str))
                return StringModsDictionary[str];
            else
                throw new ModsArgumentException(str);
        }

        public static IEnumerable<Mod> ToMods(IEnumerable<string> mods)
        {
            HashSet<Mod> result = new();
            
            if (!mods.Any())
            {
                result.Add(ToMod("NM"));
                return result;
            }

            foreach(var modString in mods)
            {
                result.Add(ToMod(modString));
            }
            return result;
        }

        public static string ToString(IEnumerable<Mod> mods)
        {
            StringBuilder sb = new();

            foreach (var mod in mods)
                sb.Append($"{mod.Name},");

            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        public static Image ToImage(IEnumerable<Mod>? mods)
        {
            if (mods == null || !mods.Any() || mods.Any(m => m.Name == "NM"))
                return null;

            Image result = new Bitmap(45 * mods.Count(), 32);
            var g = Graphics.FromImage(result);
            int i = 0;
            foreach(var mod in mods)
            { 
                g.DrawImage(mod.Image, 45 * i, 0);
                i++;
            }
            return result;
        }
    }
}
