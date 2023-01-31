using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Entites;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.Images
{
    public class ImageGenerator
    {
        private readonly Font Arista36 = new("Arista 2.0", 36);
        private readonly Dictionary<string, SolidBrush> RankBrushes = new()
        {
            {"SSH", new SolidBrush(Color.FromArgb(213, 213, 213)) },
            {"SS", new SolidBrush(Color.FromArgb(255, 217, 37)) },
            {"SH", new SolidBrush(Color.FromArgb(213, 213, 213)) },
            {"S", new SolidBrush(Color.FromArgb(255, 217, 37)) },
            {"A", new SolidBrush(Color.FromArgb(73, 255, 37)) },
            {"B", new SolidBrush(Color.FromArgb(44, 39, 255)) },
            {"C", new SolidBrush(Color.FromArgb(211, 37, 255)) },
            {"D", new SolidBrush(Color.FromArgb(255, 37, 37)) },
            {"F", new SolidBrush(Color.FromArgb(122, 122, 122)) }
        };

        private readonly Font Montserrat22 = new("Montserrat", 22);
        private readonly Font Montserrat20 = new("Montserrat", 20);
        private readonly Font Montserrat15 = new("Montserrat", 15);
        private readonly Font MontserratBold15 = new("Montserrat", 15, FontStyle.Bold);
        private readonly Font Montserrat14 = new("Montserrat", 14);
        private readonly Font MontserratBold14 = new("Montserrat", 14, FontStyle.Bold);
        private readonly Font Montserrat13 = new("Montserrat", 13);
        private readonly Font MontserratBold13 = new("Montserrat", 13);

        private readonly Font MontserratLightBold15 = new("Montserrat Light", 15, FontStyle.Bold);
        private readonly Font MontserratLightBold14 = new("Montserrat Light", 14, FontStyle.Bold);
        private readonly Font MontserratLight11 = new("Montserrat Light", 11);
        private readonly Font MontserratLightBold11 = new("Montserrat Light", 11, FontStyle.Bold);

        private readonly SolidBrush BackgroundBrush = new(Color.FromArgb(33, 34, 39));
        private readonly SolidBrush WhiteBrush = new(Color.White);
        private readonly SolidBrush Brush300 = new(Color.FromArgb(119, 197, 237));
        private readonly SolidBrush Brush100 = new(Color.FromArgb(119, 237, 138));
        private readonly SolidBrush Brush50 = new(Color.FromArgb(218, 217, 113));
        private readonly SolidBrush BrushMisses = new(Color.FromArgb(237, 119, 119));
        private readonly SolidBrush LightGrayBrush = new(Color.FromArgb(154, 160, 174));

        private readonly WebClient WebClient = new();

        public Image CreateSmallCard(BeatmapScore score)
        {
            int width = 1120;
            int height = 114;

            Image result = new Bitmap(width, height);
            var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            Image image;

            using var imgStream = new MemoryStream(WebClient.DownloadData(score.Beatmap.CoverUrl));
            image = Image.FromStream(imgStream);

            g.FillRectangle(BackgroundBrush, 0, 0, width, height);
            RectangleF frame = new Rectangle(10, 10, 146, 90);
            RectangleF imageFrame = new Rectangle(image.Width / 2 - 406, image.Height / 2 - 250, 812, 500);
            g.DrawImage(image, frame, imageFrame, GraphicsUnit.Pixel);
            g.DrawString(score.Beatmap.SongName, MontserratLightBold15, WhiteBrush, 165, 10);
            g.DrawString($"Played by {score.User.Name} on {score.Date.ToShortDateString()}", MontserratLightBold11, LightGrayBrush, 165, 35);
            g.DrawString($"{score.Beatmap.DifficultyName} {score.Beatmap.Attributes.Stars} ★", MontserratLightBold11, LightGrayBrush, 165, 55);

            g.DrawString($"{score.Accuracy}%", Montserrat13, WhiteBrush, 165, 85);
            var x = 165 + 10 + g.MeasureString($"{score.Accuracy}%", Montserrat13).Width;

            g.DrawString($"{score.MaxCombo}/{score.Beatmap.Attributes.MaxCombo}", Montserrat13, WhiteBrush, x, 85);
            x = x + 10 + g.MeasureString($"{score.MaxCombo}/{score.Beatmap.Attributes.MaxCombo}", Montserrat13).Width;

            g.DrawString(score.Count300.ToString(), Montserrat13, Brush300, x, 85);
            x += g.MeasureString(score.Count300.ToString(), Montserrat13).Width;

            g.DrawString("/", Montserrat13, LightGrayBrush, x, 85);
            x += g.MeasureString("/", Montserrat13).Width;

            g.DrawString(score.Count100.ToString(), Montserrat13, Brush100, x, 85);
            x += g.MeasureString(score.Count100.ToString(), Montserrat13).Width;

            g.DrawString("/", Montserrat13, LightGrayBrush, x, 85);
            x += g.MeasureString("/", Montserrat13).Width;

            g.DrawString(score.Count50.ToString(), Montserrat13, Brush50, x, 85);
            x += g.MeasureString(score.Count50.ToString(), Montserrat13).Width;

            g.DrawString("/", Montserrat13, LightGrayBrush, x, 85);
            x += g.MeasureString("/", Montserrat13).Width;

            g.DrawString(score.CountMisses.ToString(), Montserrat13, BrushMisses, x, 85);

            x = width - 5 - g.MeasureString($"{score.PP} PP", Montserrat20).Width;
            g.DrawString($"{score.PP} PP", Montserrat20, WhiteBrush, x, 10);

            var modsImage = ModsParser.ConvertToImage(score.Mods);
            g.DrawImage(modsImage, width - 15 - modsImage.Width, 59);

            return result;
        }

        public Image CreateFullCard(BeatmapScore score)
        {
            int width = 1080;
            int height = 376;

            Image result = new Bitmap(width, height);
            var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.FillRectangle(BackgroundBrush, 0, 0, width, height);

            using var avatarImgStream = new MemoryStream(WebClient.DownloadData(score.User.AvatarUrl));
            using var backgroundImgStream = new MemoryStream(WebClient.DownloadData(score.Beatmap.CoverUrl));

            g.DrawImage(Image.FromStream(avatarImgStream).Darkening(128), 0, 0, 204, 204);
            g.DrawImage(Image.FromStream(backgroundImgStream).Darkening(128), 204, 0, 876, 204);

            #region avatar
            string drawableString = $"#{score.User.WorldRating}";
            var x = 102 - g.MeasureString(drawableString, MontserratBold14).Width / 2;
            g.DrawString($"#{score.User.WorldRating}", MontserratBold14, WhiteBrush, x, 5);

            drawableString = $"({score.User.CountryCode} #{score.User.CountryRating})";
            x = 102 - g.MeasureString(drawableString, Montserrat14).Width / 2;
            g.DrawString(drawableString, Montserrat14, WhiteBrush, x, 30);

            drawableString = score.User.Name;
            x = 102 - g.MeasureString(drawableString, MontserratBold14).Width / 2;
            g.DrawString(drawableString, MontserratBold14, WhiteBrush, x, 155);

            drawableString = $"{score.User.PP.Separate(".")}pp";
            x = 102 - g.MeasureString(drawableString, Montserrat14).Width / 2;
            g.DrawString(drawableString, Montserrat14, WhiteBrush, x, 180);
            #endregion

            #region map
            g.DrawString($"{score.Beatmap.SongName} - {score.Beatmap.SongAuthor} [{score.Beatmap.DifficultyName}]", Montserrat15, WhiteBrush, 220, 5);
            g.DrawString($"Mapped by {score.Beatmap.Mapper.Name}", MontserratLightBold11, WhiteBrush, 220, 30);

            g.DrawString("CS:", Montserrat14, WhiteBrush, 220, 180);
            x = 220 + g.MeasureString($"CS:", Montserrat14).Width;
            g.DrawString(score.Beatmap.Attributes.CS.ToString(), MontserratBold14, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.CS.ToString(), MontserratBold14).Width;

            g.DrawString("AR:", Montserrat14, WhiteBrush, x, 180);
            x += g.MeasureString("AR:", Montserrat14).Width;
            g.DrawString(score.Beatmap.Attributes.AR.ToString(), MontserratBold14, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.AR.ToString(), MontserratBold14).Width;

            g.DrawString("OD:", Montserrat14, WhiteBrush, x, 180);
            x += g.MeasureString("OD:", Montserrat14).Width;
            g.DrawString(score.Beatmap.Attributes.OD.ToString(), MontserratBold14, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.OD.ToString(), MontserratBold14).Width;

            g.DrawString("HP:", Montserrat14, WhiteBrush, x, 180);
            x += g.MeasureString("HP:", Montserrat14).Width;
            g.DrawString(score.Beatmap.Attributes.HP.ToString(), MontserratBold14, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.HP.ToString(), MontserratBold14).Width;

            g.DrawString("Length:", Montserrat14, WhiteBrush, x, 180);
            x += g.MeasureString("Length:", Montserrat14).Width;
            drawableString = TimeSpan.FromSeconds(score.Beatmap.Attributes.Length).ToString(@"mm\:ss");
            g.DrawString(drawableString, MontserratBold14, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(drawableString, MontserratBold14).Width;

            g.DrawString("BPM:", Montserrat14, WhiteBrush, x, 180);
            x += g.MeasureString("BPM:", Montserrat14).Width;
            g.DrawString(score.Beatmap.Attributes.BPM.ToString(), MontserratBold14, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.BPM.ToString(), MontserratBold14).Width;

            g.DrawString("Stars:", Montserrat14, WhiteBrush, x, 180);
            x += g.MeasureString("Stars:", Montserrat14).Width;
            g.DrawString($"{score.Beatmap.Attributes.Stars} ★", MontserratBold14, WhiteBrush, x, 180);
            #endregion

            #region score line 1
            x = 30;
            g.DrawString("Rank", MontserratBold15, LightGrayBrush, x, 224);
            var stringLength = g.MeasureString("Rank", MontserratBold15).Width;
            drawableString = score.Rank.Last() == 'H' ? score.Rank[..^1] : score.Rank;
            var centerX = x + stringLength / 2 - g.MeasureString(drawableString, Arista36).Width / 2;
            g.DrawString(drawableString, Arista36, RankBrushes[score.Rank], centerX, 240);
            x = x + 130 + stringLength;
            
            g.DrawString("Performance", MontserratBold15, LightGrayBrush, x, 224);
            stringLength = g.MeasureString("Performance", MontserratBold15).Width;
            drawableString = $"{score.PP}PP";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat22).Width / 2;
            g.DrawString(drawableString, Montserrat22, WhiteBrush, centerX, 250);
            x = x + 130 + stringLength;         

            g.DrawString("Combo", MontserratBold15, LightGrayBrush, x, 224);
            stringLength = g.MeasureString("Combo", MontserratBold15).Width;
            drawableString = $"{score.MaxCombo}x/{score.Beatmap.Attributes.MaxCombo}x";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat22).Width / 2;
            g.DrawString(drawableString, Montserrat22, WhiteBrush, centerX, 250);
            x = x + 130 + stringLength;
            
            g.DrawString("Accuracy", MontserratBold15, LightGrayBrush, x, 224);
            stringLength = g.MeasureString("Accuracy", MontserratBold15).Width;
            drawableString = $"{score.Accuracy:F2}%";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat22).Width / 2;
            g.DrawString(drawableString, Montserrat22, WhiteBrush, centerX, 250);
            x = x + 130 + stringLength;
            
            g.DrawString("Mods", MontserratBold15, LightGrayBrush, x, 224);
            stringLength = g.MeasureString("Mods", MontserratBold15).Width;
            var modsImage = ModsParser.ConvertToImage(score.Mods);
            centerX = x + stringLength / 2 - modsImage.Width / 2;
            g.DrawImage(modsImage, centerX, 250);
            #endregion

            #region score line 2
            x = 30;
            g.DrawString("Score", MontserratBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("Score", MontserratBold13).Width;
            drawableString = score.Score.Separate(".");
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat13).Width / 2;
            g.DrawString(drawableString, Montserrat13, WhiteBrush, centerX, 350);
            x = x + 70 + stringLength;

            g.DrawString("Completion", MontserratBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("Completion", MontserratBold13).Width;
            drawableString = $"{score.Complition:F2}%";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat13).Width / 2;
            g.DrawString(drawableString, Montserrat13, WhiteBrush, centerX, 350);
            x = x + 70 + stringLength;

            g.DrawString("For FC", MontserratBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("For FC", MontserratBold13).Width;
            drawableString = "270pp";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat13).Width / 2;
            g.DrawString(drawableString, Montserrat13, WhiteBrush, centerX, 350);
            x = x + 30 + stringLength;

            g.DrawString("SS", MontserratBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("SS", MontserratBold13).Width;
            drawableString = "329pp";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat13).Width / 2;
            g.DrawString(drawableString, Montserrat13, WhiteBrush, centerX, 350);
            x = x + 70 + stringLength;

            g.DrawString("300", MontserratBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("300", MontserratBold13).Width;
            drawableString = score.Count300.ToString();
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat13).Width / 2;
            g.DrawString(drawableString, Montserrat13, Brush300, centerX, 350);
            x = x + 5 + stringLength;

            g.DrawString("100", MontserratBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("100", MontserratBold13).Width;
            drawableString = score.Count100.ToString();
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat13).Width / 2;
            g.DrawString(drawableString, Montserrat13, Brush100, centerX, 350);
            x = x + 5 + stringLength;

            g.DrawString("50", MontserratBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("50", MontserratBold13).Width;
            drawableString = score.Count50.ToString();
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat13).Width / 2;
            g.DrawString(drawableString, Montserrat13, Brush50, centerX, 350);
            x = x + 5 + stringLength;

            g.DrawString("X", MontserratBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("X", MontserratBold13).Width;
            drawableString = score.CountMisses.ToString();
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat13).Width / 2;
            g.DrawString(drawableString, Montserrat13, BrushMisses, centerX, 350);

            x = width - 140;
            g.DrawString("Played on", MontserratBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("Played on", MontserratBold13).Width;
            drawableString = score.Date.ToString("dd.MM.yyyy HH:mm");
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Montserrat13).Width / 2;
            g.DrawString(drawableString, Montserrat13, WhiteBrush, centerX, 350);

            #endregion
            return result;
        }
    }
}
