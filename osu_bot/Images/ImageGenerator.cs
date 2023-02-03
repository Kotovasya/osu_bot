using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
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
        private readonly Font Arista48 = new Font("Arista 2.0", 48);
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
            {"F", new SolidBrush(Color.FromArgb(180, 180, 180)) }
        };

        private readonly Font Rubik22 = new("Rubik", 22);
        private readonly Font Rubik20 = new("Rubik", 20);
        private readonly Font Rubik17 = new("Rubik", 17);
        private readonly Font Rubik15 = new("Rubik", 15);
        private readonly Font Rubik14 = new("Rubik", 14);
        private readonly Font Rubik13 = new("Rubik", 13);
        private readonly Font Rubik11 = new("Rubik", 11);
        

        private readonly Font RubikBold15 = new("Rubik Medium", 15);
        private readonly Font RubikBold14 = new("Rubik Medium", 14);
        private readonly Font RubikBold13 = new("Rubik Medium", 13);
        private readonly Font RubikBold11 = new("Rubik", 11, FontStyle.Bold);

        private readonly Font RubikLightBold10 = new("Rubik Light", 10, FontStyle.Bold);

        private readonly Font RubikLightBold11 = new("Rubik Light", 11, FontStyle.Bold);

        private readonly SolidBrush BackgroundLightBrush = new(Color.FromArgb(66, 68, 78));
        private readonly SolidBrush BackgroundSemilightBrush = new(Color.FromArgb(39, 41, 49));
        private readonly SolidBrush BackgroundBrush = new(Color.FromArgb(33, 34, 39));
        private readonly SolidBrush WhiteBrush = new(Color.White);     
        private readonly SolidBrush Brush300 = new(Color.FromArgb(119, 197, 237));
        private readonly SolidBrush Brush100 = new(Color.FromArgb(119, 237, 138));
        private readonly SolidBrush Brush50 = new(Color.FromArgb(218, 217, 113));
        private readonly SolidBrush BrushMisses = new(Color.FromArgb(237, 119, 119));
        private readonly SolidBrush LightGrayBrush = new(Color.FromArgb(154, 160, 174));

        private readonly Pen GraphicPen = new(Color.FromArgb(218, 217, 113), 2);
        private readonly Pen LightLinePen = new(Color.FromArgb(30, 200, 200, 200), 0.3f);

        private readonly WebClient WebClient = new();

        private string GetPlayedTimeString(DateTime date)
        {
            var diff = DateTime.Now - date;
            if (diff.Days > 0)
            {
                if (diff.Days <= 7)
                    return diff.Days == 1 ? "1 day ago" : $"{diff.Days} days ago";
                else
                    return date.ToString("dd MMMM yyyy г.");
            }

            if (diff.Hours > 0)
                return diff.Hours == 1 ? "1 hour ago" : $"{diff.Hours} hours ago";

            if (diff.Minutes > 0)
                return diff.Minutes == 1 ? "1 minute ago" : $"{diff.Minutes} minutes ago";

            if (diff.Seconds < 30)
                return "few seconds ago";

            return $"{diff.Seconds} seconds ago";
        }

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

            var drawableString = score.Rank.Last() == 'H' ? score.Rank[..^1] : score.Rank;
            g.DrawString(drawableString, Arista48, RankBrushes[score.Rank], 10, 20);
            var leftIndent = 20 + g.MeasureString(drawableString, Arista48).Width;

            RectangleF frame = new Rectangle((int)leftIndent, 14, 146, 90);
            RectangleF imageFrame = new Rectangle(image.Width / 2 - 406, image.Height / 2 - 250, 812, 500);
            g.DrawImage(image, frame, imageFrame, GraphicsUnit.Pixel);
            var x = leftIndent + 155;
            g.DrawString(score.Beatmap.SongName, Rubik15, WhiteBrush, x, 10);
            g.DrawString($"Played by {score.User.Name} {GetPlayedTimeString(score.Date)}", Rubik11, LightGrayBrush, x, 35);
            g.DrawString($"{score.Beatmap.DifficultyName} {score.Beatmap.Attributes.Stars}", Rubik11, LightGrayBrush, x, 55);
            x += g.MeasureString($"{score.Beatmap.DifficultyName} {score.Beatmap.Attributes.Stars}", Rubik11).Width;
            g.DrawString("★", RubikBold11, LightGrayBrush, x, 55);

            x = leftIndent + 155;

            g.DrawString($"{score.Accuracy}%", Rubik13, WhiteBrush, x, 85);
            x = x + 10 + g.MeasureString($"{score.Accuracy}%", Rubik13).Width;

            g.DrawString($"{score.MaxCombo}x/{score.Beatmap.Attributes.MaxCombo}x", Rubik13, WhiteBrush, x, 85);
            x = x + 10 + g.MeasureString($"{score.MaxCombo}x/{score.Beatmap.Attributes.MaxCombo}x", Rubik13).Width;

            g.DrawString(score.Count300.ToString(), Rubik13, Brush300, x, 85);
            x += g.MeasureString(score.Count300.ToString(), Rubik13).Width;

            g.DrawString("/", Rubik13, LightGrayBrush, x, 85);
            x += g.MeasureString("/", Rubik13).Width;

            g.DrawString(score.Count100.ToString(), Rubik13, Brush100, x, 85);
            x += g.MeasureString(score.Count100.ToString(), Rubik13).Width;

            g.DrawString("/", Rubik13, LightGrayBrush, x, 85);
            x += g.MeasureString("/", Rubik13).Width;

            g.DrawString(score.Count50.ToString(), Rubik13, Brush50, x, 85);
            x += g.MeasureString(score.Count50.ToString(), Rubik13).Width;

            g.DrawString("/", Rubik13, LightGrayBrush, x, 85);
            x += g.MeasureString("/", Rubik13).Width;

            g.DrawString(score.CountMisses.ToString(), Rubik13, BrushMisses, x, 85);

            x = width - 15 - g.MeasureString($"{score.PP} PP", Rubik20).Width;
            g.DrawString($"{score.PP} PP", Rubik20, WhiteBrush, x, 10);

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
            var x = 102 - g.MeasureString(drawableString, RubikBold14).Width / 2;
            g.DrawString($"#{score.User.WorldRating}", RubikBold14, WhiteBrush, x, 5);

            drawableString = $"({score.User.CountryCode} #{score.User.CountryRating})";
            x = 102 - g.MeasureString(drawableString, Rubik14).Width / 2;
            g.DrawString(drawableString, Rubik14, WhiteBrush, x, 30);

            drawableString = score.User.Name;
            x = 102 - g.MeasureString(drawableString, RubikBold14).Width / 2;
            g.DrawString(drawableString, RubikBold14, WhiteBrush, x, 155);

            drawableString = $"{score.User.PP.Separate(".")}pp";
            x = 102 - g.MeasureString(drawableString, Rubik14).Width / 2;
            g.DrawString(drawableString, Rubik14, WhiteBrush, x, 180);
            #endregion

            #region map
            g.DrawString($"{score.Beatmap.SongName} - {score.Beatmap.SongAuthor} [{score.Beatmap.DifficultyName}]", Rubik15, WhiteBrush, 220, 5);
            g.DrawString($"Mapped by {score.Beatmap.Mapper.Name}", RubikLightBold11, WhiteBrush, 220, 30);

            g.DrawString("CS:", Rubik13, WhiteBrush, 220, 180);
            x = 220 + g.MeasureString($"CS:", Rubik13).Width;
            g.DrawString(score.Beatmap.Attributes.CS.ToString(), RubikBold13, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.CS.ToString(), Rubik13).Width;

            g.DrawString("AR:", Rubik13, WhiteBrush, x, 180);
            x += g.MeasureString("AR:", Rubik13).Width;
            g.DrawString(score.Beatmap.Attributes.AR.ToString(), RubikBold13, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.AR.ToString(), RubikBold13).Width;

            g.DrawString("OD:", Rubik13, WhiteBrush, x, 180);
            x += g.MeasureString("OD:", Rubik13).Width;
            g.DrawString(score.Beatmap.Attributes.OD.ToString(), RubikBold13, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.OD.ToString(), RubikBold13).Width;

            g.DrawString("HP:", Rubik13, WhiteBrush, x, 180);
            x += g.MeasureString("HP:", Rubik13).Width;
            g.DrawString(score.Beatmap.Attributes.HP.ToString(), RubikBold13, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.HP.ToString(), RubikBold13).Width;

            g.DrawString("Length:", Rubik13, WhiteBrush, x, 180);
            x += g.MeasureString("Length:", Rubik13).Width;
            drawableString = TimeSpan.FromSeconds(score.Beatmap.Attributes.Length).ToString(@"mm\:ss");
            g.DrawString(drawableString, RubikBold13, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(drawableString, RubikBold13).Width;

            g.DrawString("BPM:", Rubik13, WhiteBrush, x, 180);
            x += g.MeasureString("BPM:", Rubik13).Width;
            g.DrawString(score.Beatmap.Attributes.BPM.ToString(), RubikBold13, WhiteBrush, x, 180);
            x = x + 5 + g.MeasureString(score.Beatmap.Attributes.BPM.ToString(), RubikBold13).Width;

            g.DrawString("Stars:", Rubik13, WhiteBrush, x, 180);
            x += g.MeasureString("Stars:", Rubik13).Width;
            g.DrawString($"{score.Beatmap.Attributes.Stars} ★", RubikBold13, WhiteBrush, x, 180);
            #endregion

            #region score line 1
            x = 70;
            g.DrawString("Rank", RubikBold15, LightGrayBrush, x, 224);
            var stringLength = g.MeasureString("Rank", RubikBold15).Width;
            drawableString = score.Rank.Last() == 'H' ? score.Rank[..^1] : score.Rank;
            var centerX = x + stringLength / 2 - g.MeasureString(drawableString, Arista36).Width / 2;
            g.DrawString(drawableString, Arista36, RankBrushes[score.Rank], centerX, 240);
            x = x + 130 + stringLength;
            
            g.DrawString("Performance", RubikBold15, LightGrayBrush, x, 224);
            stringLength = g.MeasureString("Performance", RubikBold15).Width;
            drawableString = $"{score.PP}PP";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik22).Width / 2;
            g.DrawString(drawableString, Rubik22, WhiteBrush, centerX, 250);
            x = x + 130 + stringLength;         

            g.DrawString("Combo", RubikBold15, LightGrayBrush, x, 224);
            stringLength = g.MeasureString("Combo", RubikBold15).Width;
            drawableString = $"{score.MaxCombo}x/{score.Beatmap.Attributes.MaxCombo}x";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik22).Width / 2;
            g.DrawString(drawableString, Rubik22, WhiteBrush, centerX, 250);
            x = x + 130 + stringLength;
            
            g.DrawString("Accuracy", RubikBold15, LightGrayBrush, x, 224);
            stringLength = g.MeasureString("Accuracy", RubikBold15).Width;
            drawableString = $"{score.Accuracy:F2}%";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik22).Width / 2;
            g.DrawString(drawableString, Rubik22, WhiteBrush, centerX, 250);
            x = x + 130 + stringLength;
            
            g.DrawString("Mods", RubikBold15, LightGrayBrush, x, 224);
            stringLength = g.MeasureString("Mods", RubikBold15).Width;
            var modsImage = ModsParser.ConvertToImage(score.Mods);
            centerX = x + stringLength / 2 - modsImage.Width / 2;
            g.DrawImage(modsImage, centerX, 250);
            #endregion

            #region score line 2
            x = 70;
            g.DrawString("Score", RubikBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("Score", RubikBold13).Width;
            drawableString = score.Score.Separate(".");
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik13).Width / 2;
            g.DrawString(drawableString, Rubik13, WhiteBrush, centerX, 350);
            x = x + 70 + stringLength;

            g.DrawString("Completion", RubikBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("Completion", RubikBold13).Width;
            drawableString = $"{score.Complition:F2}%";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik13).Width / 2;
            g.DrawString(drawableString, Rubik13, WhiteBrush, centerX, 350);
            x = x + 70 + stringLength;

            g.DrawString("For FC", RubikBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("For FC", RubikBold13).Width;
            drawableString = "270pp";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik13).Width / 2;
            g.DrawString(drawableString, Rubik13, WhiteBrush, centerX, 350);
            x = x + 70 + stringLength;

            g.DrawString("SS", RubikBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("SS", RubikBold13).Width;
            drawableString = "329pp";
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik13).Width / 2;
            g.DrawString(drawableString, Rubik13, WhiteBrush, centerX, 350);
            x = x + 70 + stringLength;

            g.DrawString("300", RubikBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("300", RubikBold13).Width;
            drawableString = score.Count300.ToString();
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik13).Width / 2;
            g.DrawString(drawableString, Rubik13, Brush300, centerX, 350);
            x = x + 5 + stringLength;

            g.DrawString("100", RubikBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("100", RubikBold13).Width;
            drawableString = score.Count100.ToString();
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik13).Width / 2;
            g.DrawString(drawableString, Rubik13, Brush100, centerX, 350);
            x = x + 5 + stringLength;

            g.DrawString("50", RubikBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("50", RubikBold13).Width;
            drawableString = score.Count50.ToString();
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik13).Width / 2;
            g.DrawString(drawableString, Rubik13, Brush50, centerX, 350);
            x = x + 5 + stringLength;

            g.DrawString("X", RubikBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("X", RubikBold13).Width;
            drawableString = score.CountMisses.ToString();
            centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik13).Width / 2;
            g.DrawString(drawableString, Rubik13, BrushMisses, centerX, 350);

            x = width - 215;
            g.DrawString("Played", RubikBold13, LightGrayBrush, x, 330);
            stringLength = g.MeasureString("Played", RubikBold13).Width;
            drawableString = GetPlayedTimeString(score.Date);
            centerX = centerX = x + stringLength / 2 - g.MeasureString(drawableString, Rubik13).Width / 2;
            g.DrawString(drawableString, Rubik13, WhiteBrush, centerX, 350);

            #endregion
            return result;
        }

        public Image CreateProfileCard(User user)
        {
            int width = 600;
            int height = 575;
            Image result = new Bitmap(width, height);
            var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.FillRectangle(BackgroundLightBrush, 0, 0, 286, 304);
            g.FillRectangle(BackgroundBrush, 286, 0, width, 304);
            g.FillRectangle(BackgroundSemilightBrush, 0, 304, width, height);

            //g.DrawLine(LightGrayPen, 286, 0, 286, 304);
            //g.DrawLine(LightGrayPen, 0, 304, width, 304);

            string drawableString = user.Name;
            var stringLength = g.MeasureString(drawableString, Rubik17).Width;
            var x = 276 / 2 - stringLength / 2;
            g.DrawString(user.Name, Rubik17, WhiteBrush, x, 5);

            using var avatarImgStream = new MemoryStream(WebClient.DownloadData(user.AvatarUrl));
            g.DrawImage(Image.FromStream(avatarImgStream).Darkening(32), 15, 35, 256, 256);

            var startX = 296;
            g.DrawString($"#{user.WorldRating.Separate(".")}", Rubik17, WhiteBrush, startX, 35);
            g.DrawString($"#{user.CountryRating} {user.CountryCode}", Rubik17, WhiteBrush, startX, 65);

            drawableString = "Perfomance:";
            x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
            g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 115);
            g.DrawString($"{user.PP.Separate(".")}pp", Rubik15, WhiteBrush, x, 115);

            drawableString = "Accuracy:";
            x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
            g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 145);
            g.DrawString($"{user.Accuracy}%", Rubik15, WhiteBrush, x, 145);

            drawableString = "Playcount:";
            x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
            g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 175);
            g.DrawString(user.PlayCount.Separate("."), Rubik15, WhiteBrush, x, 175);

            drawableString = "Playtime:";
            x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
            g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 205);
            g.DrawString(user.PlayTime, Rubik15, WhiteBrush, x, 205);

            drawableString = "Online:";
            x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
            g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 235);
            g.DrawString(user.LastOnline, Rubik15, WhiteBrush, x, 235);

            drawableString = "Registration:";
            x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
            g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 265);
            g.DrawString(user.DateRegistration, Rubik15, WhiteBrush, x, 265);

            g.DrawString("GLOBAL RANK HISTORY", Rubik17, WhiteBrush, 180, 310);

            int common = (int)Math.Round(user.RankHistroy.Length / 5f, MidpointRounding.ToPositiveInfinity);
            int maxRank = user.RankHistroy.Max();
            int minRank = user.RankHistroy.Min();
            float scaleX = 550f / user.RankHistroy.Length;
            float scaleY = 160f / (maxRank - minRank);

            for (int i = 0; i < user.RankHistroy.Length - 1; i++)
            {
                float y = scaleY == float.PositiveInfinity ? 440 : 350 + scaleY * (user.RankHistroy[i] - minRank);
                float y1 = scaleY == float.PositiveInfinity ? 440 : 350 + scaleY * (user.RankHistroy[i + 1] - minRank);
                if (i % common == 0 || i + 1 == user.RankHistroy.Length - 1)
                {
                    int index = i + 1 == user.RankHistroy.Length - 1 ? i + 1 : i;
                    g.DrawLine(LightLinePen, 40 + scaleX * index, index != i + 1 ? y : y1, 40 + scaleX * index, 530);
                    drawableString = $"#{user.RankHistroy[index].Separate(".")}\n{user.RankHistroy.Length - index - 1}d ago";
                    x = 40 + scaleX * index - g.MeasureString(drawableString, RubikLightBold10).Width / 2;
                    g.DrawString(drawableString, RubikLightBold10, LightGrayBrush, x, 532);
                }

                PointF start = new(40 + scaleX * i, y);
                PointF end = new(40 + scaleX * (i + 1), y1);
                g.DrawLine(GraphicPen, start, end);
            }

            return result;
        }

        public Image CreateScoresCard(IEnumerable<BeatmapScore> scores)
        {
            int width = 1120;
            int height = 136 + scores.Count() * 114;
            Image result = new Bitmap(width, height);
            var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var user = scores.First().User;
            using var avatarImgStream = new MemoryStream(WebClient.DownloadData(user.AvatarUrl));

            g.FillRectangle(BackgroundSemilightBrush, 0, 0, width, 136);
            g.DrawImage(Image.FromStream(avatarImgStream), 4, 4, 128, 128);
            g.DrawString($"#{user.Name}", Rubik17, WhiteBrush, 136, 30);
            g.DrawString($"#{user.WorldRating}", Rubik17, WhiteBrush, 136, 60);
            g.DrawString($"({user.CountryCode} #{user.CountryRating})", Rubik17, WhiteBrush, 136, 90);

            int i = 0;
            foreach (var score in scores)
            {
                var y = 136 + i * 114;
                var scoreImage = CreateSmallCard(score);                
                g.DrawImage(scoreImage, 0, y);
                if (i != 0)
                    g.DrawLine(LightLinePen, 0, y, width, y);
                i++;
            }

            return result;
        }
    }
}
