using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites
{
    public class Beatmap
    {
        public long Id { get; set; }
        public string SongName { get; set; }
        public string DifficultyName { get; set; }
        public string CoverUrl { get; set; }
        public string Status { get; set; }
        public string SongAuthor { get; set; }
        public User Mapper { get; set; }
        public BeatmapAttribute Attributes { get; set; }
    }

    public class BeatmapAttribute
    {
        public double Stars { get; set; }
        public int MaxCombo { get; set; }
        public int BPM { get; set; }
        public double CS { get; set; }
        public double AR { get; set; }
        public double OD { get; set; }
        public double HP { get; set; }
    }
}
