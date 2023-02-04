using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites
{
    public class ScoreInfo
    {
        public long Id { get; set; }
        public int Score { get; set; }
        public double Accuracy { get; set; }
        public DateTime Date { get; set; }
        public int MaxCombo { get; set; }
        public int PP { get; set; }
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public int Count300 { get; set; }
        public int CountMisses { get; set; }
        public double Complition { get; set; }
        public string Rank { get; set; }
        public Mods Mods { get; set; }
        public Beatmap Beatmap { get; set; }
        public User User { get; set; }
    }
}
