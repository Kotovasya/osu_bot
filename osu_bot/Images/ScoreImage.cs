using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Entites;

namespace osu_bot.Images
{
    public abstract class ScoreImage
    {
        protected ScoreImage(BeatmapScore score)
        {
            Score = score;
        }

        protected abstract int Width { get; }
        protected abstract int Height { get; }
        protected BeatmapScore Score { get; private set; }

        protected readonly SolidBrush BackgroundBrush = new(Color.FromArgb(63, 64, 69));
        protected readonly SolidBrush WhiteBrush = new(Color.White);
        protected readonly SolidBrush Brush300 = new(Color.FromArgb(119, 197, 237));
        protected readonly SolidBrush Brush100 = new(Color.FromArgb(119, 237, 138));
        protected readonly SolidBrush Brush50 = new(Color.FromArgb(218, 217, 113));
        protected readonly SolidBrush BrushMisses = new(Color.FromArgb(237, 119, 119));
        protected readonly SolidBrush LightGrayBrush = new(Color.FromArgb(154, 160, 174));

        protected readonly WebClient WebClient = new();
    }
}
