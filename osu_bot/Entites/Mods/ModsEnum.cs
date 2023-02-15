using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Mods
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
}
