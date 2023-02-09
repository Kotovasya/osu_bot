using osu_bot.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.Entites
{
    [Flags]
    public enum Mods
    {
        NM = 0,
        NF = 1 << 0,    //No fail
        EZ = 1 << 1,    //Easy
        TD = 1 << 2,    //TouchDevice
        HD = 1 << 3,    //Hidden
        HR = 1 << 4,    //HardRock
        SD = 1 << 5,    //SuddenDeath
        DT = 1 << 6,    //DoubleTime
        RX = 1 << 7,    //Relax
        HT = 1 << 8,    //HalfTime
        NC = 1 << 9,    //Nightcore
        FL = 1 << 10,   //Flashlight
        AT = 1 << 11,   //Autoplay
        SO = 1 << 12,   //SpunOut
        AP = 1 << 13,   //Aotupilot
        PF = 1 << 14,   //Perfect
        K4 = 1 << 15,
        K5 = 1 << 16,
        K6 = 1 << 17,
        K7 = 1 << 18,
        K8 = 1 << 19,
        FI = 1 << 20,
        RN = 1 << 21,
        CN = 1 << 22,   //Cinema
        TP = 1 << 23,
        K9 = 1 << 24,
        K10 = 1 << 25,
        K1 = 1 << 26,
        K3 = 1 << 27,
        K2 = 1 << 28,
        V2 = 1 << 29,   //ScoreV2
        MR = 1 << 30,
        ALL = 1 << 31
    }

    public static class ModsParser
    {
        public static int ConvertToInt(Mods mods)
        {
            return (int)mods;
        }

        public static int ConvertToInt(string mods)
        {
            if (string.IsNullOrWhiteSpace(mods)) return 0;
            if (mods.Length > 2)
            {
                var str = Regex.Replace(mods, ".{2}", "$0,");
                return (int)Enum.Parse(typeof(Mods), str.AsSpan(0, str.Length - 1));
            }
            else
                return (int)Enum.Parse(typeof(Mods), mods);
        }

        public static string ConvertToString(Mods mods)
        {
            return mods.ToString();
        }

        public static string ConvertToString(int mods)
        {
            return ConvertToString((Mods)mods);
        }

        public static Mods ConvertToMods(string mods)
        {
            try
            {
                return (Mods)ConvertToInt(mods);
            }
            catch
            {
                throw new ModsArgumentException();
            }
        }

        public static Mods ConvertToMods(int mods)
        {
            return (Mods)mods;
        }

        public static Image ConvertToImage(Mods mods)
        {
            if (mods == Mods.NM)
                return new Bitmap(0, 0);

            var modsArray = mods.ToString().Split(", ");
            Image result = new Bitmap(45 * modsArray.Length, 32);
            var g = Graphics.FromImage(result);
            for (int i = 0; i < modsArray.Length; i++)
            {
                var modFile = (Image)Resources.ResourceManager.GetObject($"{modsArray[i]}");
                g.DrawImage(modFile, 45 * i, 0);
            }
            return result;
        }
    }
}
