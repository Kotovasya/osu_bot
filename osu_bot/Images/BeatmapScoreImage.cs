using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Images
{
    public class BeatmapScoreImage : ScoreImage
    {
        public BeatmapScoreImage(BeatmapScore score) : base(score)
        {

        }

        protected override int Width => 1120;
        protected override int Height => 114;

        private static readonly int _widthFullCard = 1080;
        private static readonly int _heightFullCard = 430;

        private readonly Font PerfomanceFont = new("Montserrat", 20);
        private readonly Font TitleFont = new("Montserrat Light", 15, FontStyle.Bold);
        private readonly Font DigitFont = new("Montserrat", 11);
        private readonly Font InfoFont = new("Montserrat Light", 11, FontStyle.Bold);

        public BeatmapScore Score { get; set; }

        public Image CreateSmallCard()
        {
            Image result = new Bitmap(Width, Height);
            var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Image image;

            using var imgStream = new MemoryStream(WebClient.DownloadData(Score.Beatmap.CoverUrl));
            image = Image.FromStream(imgStream);

            g.FillRectangle(BackgroundBrush, 0, 0, Width, Height);
            RectangleF frame = new Rectangle(10, 10, 146, 90);
            RectangleF imageFrame = new Rectangle(image.Width / 2 - 406, image.Height / 2 - 250, 812, 500);
            g.DrawImage(image, frame, imageFrame, GraphicsUnit.Pixel);
            g.DrawString(Score.Beatmap.SongName, TitleFont, WhiteBrush, 165, 10);
            g.DrawString($"Played by {Score.User.Name} on {Score.Date.ToShortDateString()}", InfoFont, LightGrayBrush, 165, 35);
            g.DrawString($"{Score.Beatmap.DifficultyName} {Score.Beatmap.Attributes.Stars} ★", InfoFont, LightGrayBrush, 165, 55);

            g.DrawString($"{Score.Accuracy}%", DigitFont, WhiteBrush, 165, 85);
            var x = 165 + 10 + g.MeasureString($"{Score.Accuracy}%", DigitFont).Width;

            g.DrawString($"{Score.MaxCombo}/{Score.Beatmap.Attributes.MaxCombo}", DigitFont, WhiteBrush, x, 85);
            x = x + 10 + g.MeasureString($"{Score.MaxCombo}/{Score.Beatmap.Attributes.MaxCombo}", DigitFont).Width;

            g.DrawString(Score.Count300.ToString(), DigitFont, Brush300, x, 85);
            x += g.MeasureString(Score.Count300.ToString(), DigitFont).Width;

            g.DrawString("/", DigitFont, LightGrayBrush, x, 85);
            x += g.MeasureString("/", DigitFont).Width;

            g.DrawString(Score.Count100.ToString(), DigitFont, Brush100, x, 85);
            x += g.MeasureString(Score.Count100.ToString(), DigitFont).Width;

            g.DrawString("/", DigitFont, LightGrayBrush, x, 85);
            x += g.MeasureString("/", DigitFont).Width;

            g.DrawString(Score.Count50.ToString(), DigitFont, Brush50, x, 85);
            x += g.MeasureString(Score.Count50.ToString(), DigitFont).Width;

            g.DrawString("/", DigitFont, LightGrayBrush, x, 85);
            x += g.MeasureString("/", DigitFont).Width;

            g.DrawString(Score.CountMisses.ToString(), DigitFont, BrushMisses, x, 85);

            x = Width - 5 - g.MeasureString($"{Score.PP} PP", PerfomanceFont).Width;
            g.DrawString($"{Score.PP} PP", PerfomanceFont, WhiteBrush, x, 10);

            if (Score.Mods != Mods.NM)
            {
                var mods = Score.Mods.ToString().Split(", ");
                for (int i = 0; i < mods.Length; i++)
                {
                    x = Width - 50 * (i + 1);
                    var modFile = (Image)Resources.ResourceManager.GetObject($"{mods[i]}");
                    g.DrawImage(modFile, x, 59);
                }
            }
            return result;
        }
    }
}
