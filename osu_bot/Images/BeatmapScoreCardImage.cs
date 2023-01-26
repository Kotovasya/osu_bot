using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Entites;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.Images
{
    public class BeatmapScoreCardImage : ScoreImage
    {
        public BeatmapScoreCardImage(BeatmapScore score) 
            : base(score)
        {

        }

        protected override int Width => 1080;
        protected override int Height => 430;

        public Image CreateFullCard()
        {
            Image result = new Bitmap(Width, Height);
            var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRectangle(BackgroundBrush, 0, 0, Width, Height);

            using var avatarImgStream = new MemoryStream(WebClient.DownloadData(Score.User.AvatarUrl));
            using var backgroundImgStream = new MemoryStream(WebClient.DownloadData(Score.Beatmap.CoverUrl));

            g.DrawImage(Image.FromStream(avatarImgStream), 0, 0, 280, 280);
            g.DrawImage(Image.FromStream(backgroundImgStream), 280, 0, 800, 280);

            g.DrawString();

            return result;
        }

        
    }
}
