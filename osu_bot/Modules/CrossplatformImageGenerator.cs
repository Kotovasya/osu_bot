using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using static System.Formats.Asn1.AsnWriter;
using osu_bot.Assets;

namespace osu_bot.Modules
{
    public class CrossplatformImageGenerator
    {
        private class StringsLinker
        {
            public string? FirstString { get; set; }
            public string? SecondString { get; set; }

            public float X { get; set; }
            public float Y1 { get; set; }
            public float Y2 { get; set; }

            public SKPaint FirstPaint { get; } = new SKPaint() { IsAntialias = true, FilterQuality = SKFilterQuality.High, LcdRenderText = true };
            public SKPaint SecondPaint { get; } = new SKPaint() { IsAntialias = true, FilterQuality = SKFilterQuality.High, LcdRenderText = true };

            public bool IsCenter { get; set; }
            public float StringSpacing { get; set; }

            public StringsLinker(SKPaint firstPaint, SKPaint secondPaint, bool isCenter = true)
            {
                CopyFonts(firstPaint, secondPaint);
                IsCenter = isCenter;
            }

            public StringsLinker(SKPaint firstPaint, SKPaint secondPaint, float stringSpacing)
            {
                CopyFonts(firstPaint, secondPaint);
                StringSpacing = stringSpacing;
            }

            private void CopyFonts(SKPaint paint1, SKPaint paint2)
            {
                FirstPaint.SetColor(paint1.Color).SetTypeface(paint1.Typeface).SetSize(paint1.TextSize);
                SecondPaint.SetColor(paint2.Color).SetTypeface(paint2.Typeface).SetSize(paint2.TextSize);
            }

            public StringsLinker SetPositions(float x, float y1, float y2)
            {
                X = x;
                Y1 = y1;
                Y2 = y2;
                return this;
            }

            public StringsLinker SetStrings(string firstString, string secondString)
            {
                FirstString = firstString;
                SecondString = secondString;
                return this;
            }

            public float Draw(SKCanvas canvas)
            {
                canvas.DrawText(FirstString, X, Y1, FirstPaint);
                if (IsCenter)
                {
                    float centerX = X + FirstPaint.MeasureText(FirstString) / 2;
                    float X2 = centerX - SecondPaint.MeasureText(SecondString) / 2;
                    canvas.DrawText(SecondString, X2, Y2, SecondPaint);
                    return X + FirstPaint.MeasureText(FirstString);
                }
                else
                {
                    float X2 = X + FirstPaint.MeasureText(FirstString) + StringSpacing;
                    canvas.DrawText(SecondString, X2, Y2, SecondPaint);
                    return X2 + SecondPaint.MeasureText(SecondString);
                }
            }
        }

        private const int STAR_UNICODE = 9733;
        private const int TRIANGLE_UNICODE = 9650;

        public static CrossplatformImageGenerator Instance { get; } = new();

        private readonly SKTypeface _starTypeface = SKFontManager.Default.MatchCharacter(STAR_UNICODE);
        private readonly SKTypeface _triangleTypeface = SKFontManager.Default.MatchCharacter(TRIANGLE_UNICODE);

        private readonly SKImageFilter _imageDarkingFilter = SKImageFilter.CreateColorFilter(
            SKColorFilter.CreateBlendMode(new SKColor(0, 0, 0, 192), SKBlendMode.Darken));

        private readonly SKPaint _paint = new()
        {
            FilterQuality = SKFilterQuality.High,
            IsAntialias = true,
            LcdRenderText = true,
        };

        #region SKColors initializations
        private readonly SKColor _backgroundColor = new(33, 34, 39);
        private readonly SKColor _backgroundLightColor = new(66, 68, 78);
        private readonly SKColor _backgroundSemilightColor = new(39, 41, 49);
        private readonly SKColor _whiteColor = new(255, 255, 255);
        private readonly SKColor _color300 = new(119, 197, 237);
        private readonly SKColor _color100 = new(119, 237, 138);
        private readonly SKColor _color50 = new(218, 217, 113);
        private readonly SKColor _colorMisses = new(237, 119, 119);
        private readonly SKColor _lightGrayColor = new(154, 160, 174);

        private static readonly Dictionary<string, SKColor> Rankcolors = new()
        {
            { "XH", new SKColor(221, 221, 221) },
            { "X", new SKColor(255, 190, 60) },
            { "SH",new SKColor(221, 221, 221) },
            { "S", new SKColor(255, 190, 60) },
            { "A", new SKColor(90, 200, 10) },
            { "B", new SKColor(3, 105, 241) },
            { "C", new SKColor(208, 23, 228) },
            { "D", new SKColor(226, 0, 0) },
            { "F", new SKColor(226, 0, 0) }
        };

        #endregion

        #region SKTypefaces initialization
        private readonly SKTypeface _secularOneTypeface =
            SKTypeface.FromFamilyName("Secular One", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

        private readonly SKTypeface _rubikTypeface =
            SKTypeface.FromFamilyName("Rubik", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

        private readonly SKTypeface _rubikLightTypeface =
            SKTypeface.FromFamilyName("Rubik", SKFontStyleWeight.Light, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

        private readonly SKTypeface _rubikBoldTypeface =
            SKTypeface.FromFamilyName("Rubik", SKFontStyleWeight.Medium, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

        #endregion

        //private readonly Dictionary<RankStatus, SKImage> _rankStatus = new()
        //{
        //    { RankStatus.Graveyard, Resources.GraveyardStatus.ToSKImage()  },
        //    { RankStatus.Wip, Resources.GraveyardStatus.ToSKImage()  },
        //    { RankStatus.Pending, Resources.GraveyardStatus.ToSKImage()  },
        //    { RankStatus.Ranked, Resources.RatingStatus.ToSKImage()  },
        //    { RankStatus.Approved, Resources.ApprovedStatus.ToSKImage()  },
        //    { RankStatus.Qualified, Resources.ApprovedStatus.ToSKImage()  },
        //    { RankStatus.Loved, Resources.LovedStatus.ToSKImage()  },
        //};

        private readonly WebClient _webClient = new();

        private CrossplatformImageGenerator()
        {
            
        }

        private static string GetPlayedTimeString(DateTime date)
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

            if (diff.Seconds > 30)
                return $"{diff.Seconds} seconds ago";

            return "few seconds ago";
        }

        public async Task<SKImage> CreateSmallCardAsync(ScoreInfo score, bool showNick)
        {
            int width = 1000;
            int height = 114;

            SKImageInfo imageInfo = new(width, height);
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                SKCanvas canvas = surface.Canvas;

                canvas.Clear(_backgroundColor);

                #region Rank, Image, MapInfo

                float x = 100f;

                byte[] data = await _webClient.DownloadDataTaskAsync(score.Beatmap.CoverUrl);
                var image = SKImage.FromEncodedData(data);
                var sourceRect = new SKRect() { Location = new SKPoint(image.Width / 2 - 406, image.Height / 2 - 250), Size = new SKSize(812, 500) };
                var destRect = new SKRect() { Location = new SKPoint(x, 14), Size = new SKSize(146, 90) };
                canvas.DrawImage(image, sourceRect, destRect, _paint);

                string drawableString = score.Rank.Last() == 'H' ? score.Rank[..^1] : score.Rank;

                _paint.SetColor(Rankcolors[score.Rank]).SetTypeface(_secularOneTypeface).SetSize(64);

                x = x / 2 - _paint.MeasureText(drawableString) / 2;
                canvas.DrawText(drawableString, x, 78, _paint);

                x = 260;

                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(20);
                canvas.DrawText(score.Beatmap.Title, new SKRect() { Location = new(x, 30), Size = new(575, 20) }, _paint);

                drawableString = showNick ? $"Played by {score.User.Name} {GetPlayedTimeString(score.Date)}" : $"Played {GetPlayedTimeString(score.Date)}";
                _paint.SetColor(_lightGrayColor).SetSize(15);
                canvas.DrawText(drawableString, x, 55, _paint);

                drawableString = $"{score.Beatmap.DifficultyName} {score.Beatmap.Attributes.Stars:0.00}";
                canvas.DrawText(drawableString, x, 75, _paint);

                x += _paint.MeasureText(drawableString);

                _paint.SetTypeface(_starTypeface).SetSize(22);
                canvas.DrawText("★", x, 76, _paint);
                #endregion

                #region Play stats
                x = 260;
                float y = 101f;

                drawableString = $"{score.Accuracy:0.00}%";
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(16);
                canvas.DrawText(drawableString, x, y, _paint);
                x += 20 + _paint.MeasureText(drawableString);

                drawableString = $"{score.MaxCombo}x/{score.Beatmap.Attributes.MaxCombo}x";
                canvas.DrawText(drawableString, x, y, _paint);
                x += 20 + _paint.MeasureText(drawableString);

                drawableString = score.Count300.ToString();
                _paint.SetColor(_color300);
                canvas.DrawText(drawableString, x, y, _paint);
                x += 5 + _paint.MeasureText(drawableString);

                drawableString = "/";
                _paint.SetColor(_whiteColor);
                canvas.DrawText(drawableString, x, y, _paint);
                x += 5 + _paint.MeasureText(drawableString);

                drawableString = score.Count100.ToString();
                _paint.SetColor(_color100);
                canvas.DrawText(drawableString, x, y, _paint);
                x += 5 + _paint.MeasureText(drawableString);

                drawableString = "/";
                _paint.SetColor(_whiteColor);
                canvas.DrawText(drawableString, x, y, _paint);
                x += 5 + _paint.MeasureText(drawableString);

                drawableString = score.Count50.ToString();
                _paint.SetColor(_color50);
                canvas.DrawText(drawableString, x, y, _paint);
                x += 5 + _paint.MeasureText(drawableString);

                drawableString = "/";
                _paint.SetColor(_whiteColor);
                canvas.DrawText(drawableString, x, y, _paint);
                x += 5 + _paint.MeasureText(drawableString);

                drawableString = score.CountMisses.ToString();
                _paint.SetColor(_colorMisses);
                canvas.DrawText(drawableString, x, y, _paint);

                if (score.HitObjects != score.Beatmap.Attributes.TotalObjects)
                {
                    float hits = score.HitObjects * 1.0f / score.Beatmap.Attributes.TotalObjects * 100.0f;
                    drawableString = $"{hits:F2}";
                    _paint.SetColor(_whiteColor);
                    x = 50 - _paint.MeasureText(drawableString) / 2;
                    canvas.DrawText(drawableString, x, y, _paint);
                }

                int pp = score.PP != null ? (int)score.PP : PerfomanceCalculator.Calculate(score);
                drawableString = $"{pp} PP";
                _paint.SetColor(_whiteColor).SetSize(30);
                x = width - 15 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, x, 40, _paint);

                SKImage? modsImage = ModsConverter.ToImage(score.Mods);
                if (modsImage != null)
                    canvas.DrawImage(modsImage, width - 15 - modsImage.Width, 59);
                #endregion
                
                return surface.Snapshot();
            }
        }

        public async Task<SKImage> CreateFullCardAsync(ScoreInfo score)
        {
            int width = 1080;
            int height = 376;

            SKImageInfo imageInfo = new(width, height);
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                SKCanvas canvas = surface.Canvas;

                canvas.Clear(_backgroundColor);

                #region Background map image
                byte[] data = await _webClient.DownloadDataTaskAsync(score.Beatmap.CoverUrl);
                var sourceRect = new SKRect() { Location = new SKPoint(0, 36), Size = new SKSize(1800, 428) };
                var destRect = new SKRect() { Location = new SKPoint(204, 0), Size = new SKSize(876, 204) };
                var image = SKImage.FromEncodedData(data);
                var imageSize = new SKRectI(0, 0, image.Width, image.Height);
                image = image.ApplyImageFilter(_imageDarkingFilter, imageSize, imageSize, out SKRectI _, out SKPointI _);
                canvas.DrawImage(image, sourceRect, destRect, _paint);
                #endregion

                #region Avatar image
                data = await _webClient.DownloadDataTaskAsync(score.User.AvatarUrl);
                image = SKImage.FromEncodedData(data);
                imageSize = new SKRectI(0, 0, image.Width, image.Height);
                image = image.ApplyImageFilter(_imageDarkingFilter, imageSize, imageSize, out SKRectI _, out SKPointI _);
                destRect = new SKRect() { Location = new SKPoint(0, 0), Size = new SKSize(204, 204) };
                canvas.DrawImage(image, imageSize, destRect, _paint);

                SKPaint paint1 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(18);
                SKPaint paint2 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikLightTypeface).SetSize(18);
                StringsLinker stringLinker = new(paint1, paint2, true);

                string drawableString1 = $"#{score.User.WorldRating}";
                string drawableString2 = $"({score.User.CountryCode} #{score.User.CountryRating})";      
                float x = 102 - paint1.MeasureText(drawableString1) / 2;

                stringLinker.SetStrings(drawableString1, drawableString2)
                    .SetPositions(x, 23, 48)
                    .Draw(canvas);

                drawableString1 = score.User.Name;
                drawableString2 = $"{score.User.PP.Separate(".")}pp";
                x = 102 - paint1.MeasureText(drawableString1) / 2;

                stringLinker.SetStrings(drawableString1, drawableString2)
                    .SetPositions(x, 173, 198)
                    .Draw(canvas);
                #endregion

                #region Map info
                x = 218;
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(20);
                string drawableString = $"{score.Beatmap.Title} [{score.Beatmap.DifficultyName}]";
                canvas.DrawText(drawableString, x, 23, _paint);
               
                drawableString = $"{score.Beatmap.Artist}";
                _paint.SetSize(18);
                canvas.DrawText(drawableString, x, 48, _paint);

                drawableString = $"Mapped by {score.Beatmap.MapperName}";
                _paint.SetSize(15);
                canvas.DrawText(drawableString, x, 73, _paint);

                float y = 195;
                float columnSpacing = 18;
                float wordSpacing = 4;
                paint1 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikLightTypeface).SetSize(18);
                paint2 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(18);
                stringLinker = new StringsLinker(paint1, paint2, wordSpacing);

                drawableString2 = score.Beatmap.Attributes.BaseCS.ToString("0.0");
                x = stringLinker.SetStrings("CS:", drawableString2)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                if (score.Beatmap.Attributes.CS != score.Beatmap.Attributes.BaseCS)
                {
                    x += 2;
                    drawableString = $"{score.Beatmap.Attributes.CS:0.0}";
                    _paint.SetColor(_colorMisses).SetTypeface(_rubikBoldTypeface).SetSize(12);
                    canvas.DrawText(drawableString, x, y - 4, _paint);

                    x += _paint.MeasureText(drawableString);
                    drawableString = "▲";
                    _paint.SetTypeface(_triangleTypeface);
                    canvas.DrawText(drawableString, x, y - 4, _paint);
                    x += _paint.MeasureText(drawableString);
                }

                x += columnSpacing;

                drawableString2 = score.Beatmap.Attributes.BaseAR.ToString("0.0");
                x = stringLinker.SetStrings("AR:", drawableString2)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                if (score.Beatmap.Attributes.AR != score.Beatmap.Attributes.BaseAR)
                {
                    x += 2;
                    drawableString = $"{score.Beatmap.Attributes.AR:0.0}";
                    _paint.SetColor(_colorMisses).SetTypeface(_rubikBoldTypeface).SetSize(12);
                    canvas.DrawText(drawableString, x, y - 4, _paint);

                    x += _paint.MeasureText(drawableString);
                    drawableString = "▲";
                    _paint.SetTypeface(_triangleTypeface);
                    canvas.DrawText(drawableString, x, y - 4, _paint);
                    x += _paint.MeasureText(drawableString);
                }

                x += columnSpacing;

                drawableString2 = score.Beatmap.Attributes.BaseOD.ToString("0.0");
                x = stringLinker.SetStrings("OD:", drawableString2)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                if (score.Beatmap.Attributes.OD != score.Beatmap.Attributes.BaseOD)
                {
                    x += 2;
                    drawableString = $"{score.Beatmap.Attributes.OD:0.0}";
                    _paint.SetColor(_colorMisses).SetTypeface(_rubikBoldTypeface).SetSize(12);
                    canvas.DrawText(drawableString, x, y - 4, _paint);

                    x += _paint.MeasureText(drawableString);
                    drawableString = "▲";
                    _paint.SetTypeface(_triangleTypeface);
                    canvas.DrawText(drawableString, x, y - 4, _paint);
                    x += _paint.MeasureText(drawableString);
                }

                x += columnSpacing;

                drawableString2 = score.Beatmap.Attributes.BaseHP.ToString("0.0");
                x = stringLinker.SetStrings("HP:", drawableString2)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                if (score.Beatmap.Attributes.HP != score.Beatmap.Attributes.BaseHP)
                {
                    x += 2;
                    drawableString = $"{score.Beatmap.Attributes.HP:0.0}";
                    _paint.SetColor(_colorMisses).SetTypeface(_rubikBoldTypeface).SetSize(12);
                    canvas.DrawText(drawableString, x, y - 4, _paint);

                    x += _paint.MeasureText(drawableString);
                    drawableString = "▲";
                    _paint.SetTypeface(_triangleTypeface);
                    canvas.DrawText(drawableString, x, y - 4, _paint);
                    x += _paint.MeasureText(drawableString);
                }

                x += columnSpacing;

                drawableString2 = TimeSpan.FromSeconds(score.Beatmap.Attributes.Length).ToString(@"mm\:ss");
                x = stringLinker.SetStrings("Length:", drawableString2)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString2 = score.Beatmap.Attributes.BPM.ToString();
                x = stringLinker.SetStrings("BPM:", drawableString2)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString2 = $"{score.Beatmap.Attributes.Stars:0.00}";
                x = stringLinker.SetStrings("Stars:", drawableString2)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                drawableString = "★";
                _paint.SetColor(_whiteColor).SetTypeface(_starTypeface).SetSize(20);
                canvas.DrawText(drawableString, x, y, _paint);

                x += columnSpacing;

                //Отрисовать значок статуса карты
                #endregion

                #region Score line 1
                x = 70;
                y = 242;
                columnSpacing = 130;

                drawableString = "Rank";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(18);
                canvas.DrawText(drawableString, x, y, _paint);
                float stringLength = _paint.MeasureText(drawableString);

                drawableString = score.Rank.Last() == 'H' ? score.Rank[..^1] : score.Rank;
                _paint.SetColor(Rankcolors[score.Rank]).SetTypeface(_secularOneTypeface).SetSize(48);
                float centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 16, _paint);
                x += columnSpacing + stringLength;

                drawableString = "Perfomance";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(18);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                int pp = score.PP != null ? (int)score.PP : PerfomanceCalculator.Calculate(score);
                drawableString = $"{pp}PP";
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(48);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 16, _paint);
                x += columnSpacing + stringLength;

                drawableString = "Perfomance";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(18);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = $"{score.MaxCombo}x/{score.Beatmap.Attributes.MaxCombo}x";
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(48);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 16, _paint);
                x += columnSpacing + stringLength;

                drawableString = "Accuracy";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(18);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = $"{score.Accuracy:F2}%";
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(48);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 16, _paint);
                x += columnSpacing + stringLength;

                drawableString = "Mods";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(18);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                SKImage? modsImage = ModsConverter.ToImage(score.Mods);
                if (modsImage != null)
                {
                    centerX = x + stringLength / 2 - modsImage.Width / 2;
                    if (centerX + modsImage.Width < width)
                        canvas.DrawImage(modsImage, centerX, 250, _paint);
                    else
                        canvas.DrawImage(modsImage, width - 5 - modsImage.Width, 250, _paint);
                }
                #endregion

                #region Score line 2
                x = 70;
                y = 345;
                columnSpacing = 70;

                drawableString = "Score";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(15);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = score.Score.Separate(".");
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 20, _paint);
                x += columnSpacing + stringLength;

                drawableString = "Hit objects";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(15);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                float hits = score.HitObjects * 1.0f / score.Beatmap.Attributes.TotalObjects * 100.0f;
                drawableString = $"{hits:F2}";
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 20, _paint);
                x += columnSpacing + stringLength;

                drawableString = "For FC";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(15);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = $"{PerfomanceCalculator.Calculate(score, isFullCombo: true)}pp";
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 20, _paint);
                x += columnSpacing + stringLength;

                drawableString = "For SS";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(15);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = $"{PerfomanceCalculator.Calculate(score, isFullCombo: true, isPerfect: true)}pp";
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 20, _paint);
                x += columnSpacing + stringLength;

                drawableString = "300";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(15);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = score.Count300.ToString();
                _paint.SetColor(_color300).SetTypeface(_rubikTypeface);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 20, _paint);
                x += columnSpacing + stringLength;

                drawableString = "100";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(15);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = score.Count100.ToString();
                _paint.SetColor(_color100).SetTypeface(_rubikTypeface);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 20, _paint);
                x += columnSpacing + stringLength;

                drawableString = "50";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(15);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = score.Count50.ToString();
                _paint.SetColor(_color50).SetTypeface(_rubikTypeface);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 20, _paint);
                x += columnSpacing + stringLength;

                drawableString = "X";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(15);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = score.CountMisses.ToString();
                _paint.SetColor(_colorMisses).SetTypeface(_rubikTypeface);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 20, _paint);

                x = width - 215;
                drawableString = "Played";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikBoldTypeface).SetSize(15);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                drawableString = GetPlayedTimeString(score.Date);
                _paint.SetColor(_colorMisses).SetTypeface(_rubikTypeface);
                centerX = x + stringLength / 2 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, centerX, y + 20, _paint);
                #endregion

                return surface.Snapshot();
            }        
        }

        //public static Image CreateProfileCard(User user)
        //{
        //    int width = 600;
        //    int height = 575;
        //    Image result = new Bitmap(width, height);
        //    var g = Graphics.FromImage(result);
        //    g.SmoothingMode = SmoothingMode.AntiAlias;
        //    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        //    g.FillRectangle(BackgroundLightBrush, 0, 0, 286, 304);
        //    g.FillRectangle(BackgroundBrush, 286, 0, width, 304);
        //    g.FillRectangle(BackgroundSemilightBrush, 0, 304, width, height);

        //    #region Avatar
        //    string drawableString = user.Name;
        //    var stringLength = g.MeasureString(drawableString, Rubik17).Width;
        //    var x = 276 / 2 - stringLength / 2;
        //    g.DrawString(user.Name, Rubik17, WhiteBrush, x, 5);

        //    using var avatarImgStream = new MemoryStream(WebClient.DownloadData(user.AvatarUrl));
        //    g.DrawImage(Image.FromStream(avatarImgStream).Darkening(32), 15, 35, 256, 256);
        //    #endregion

        //    #region Stats
        //    var startX = 296;
        //    g.DrawString($"#{user.WorldRating.Separate(".")}", Rubik17, WhiteBrush, startX, 35);
        //    g.DrawString($"#{user.CountryRating} {user.CountryCode}", Rubik17, WhiteBrush, startX, 65);

        //    drawableString = "Perfomance:";
        //    x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
        //    g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 115);
        //    g.DrawString($"{user.PP.Separate(".")}pp", Rubik15, WhiteBrush, x, 115);

        //    drawableString = "Accuracy:";
        //    x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
        //    g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 145);
        //    g.DrawString($"{user.Accuracy:0.00}%", Rubik15, WhiteBrush, x, 145);

        //    drawableString = "Playcount:";
        //    x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
        //    g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 175);
        //    g.DrawString(user.PlayCount.Separate("."), Rubik15, WhiteBrush, x, 175);

        //    drawableString = "Playtime:";
        //    x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
        //    g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 205);
        //    g.DrawString($"{user.PlayTime.Days}d {user.PlayTime.Hours}h {user.PlayTime.Minutes}m {user.PlayTime.Seconds}s", Rubik15, WhiteBrush, x, 205);

        //    drawableString = "Online:";
        //    x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
        //    g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 235);
        //    g.DrawString(user.LastOnline != null ? GetPlayedTimeString(user.LastOnline.Value) : "скрыто", Rubik15, WhiteBrush, x, 235);

        //    drawableString = "Registration:";
        //    x = startX + 2 + g.MeasureString(drawableString, Rubik15).Width;
        //    g.DrawString(drawableString, Rubik15, LightGrayBrush, startX, 265);
        //    g.DrawString(user.DateRegistration.ToString("dd MM yyyy г."), Rubik15, WhiteBrush, x, 265);
        //    #endregion

        //    #region Rank history
        //    g.DrawString("GLOBAL RANK HISTORY", Rubik17, WhiteBrush, 180, 310);

        //    int common = (int)Math.Round(user.RankHistory.Length / 5f, MidpointRounding.ToPositiveInfinity);
        //    int maxRank = user.RankHistory.Max();
        //    int minRank = user.RankHistory.Min();
        //    float scaleX = 550f / user.RankHistory.Length;
        //    float scaleY = 160f / (maxRank - minRank);

        //    for (int i = 0; i < user.RankHistory.Length - 1; i++)
        //    {
        //        float y = scaleY == float.PositiveInfinity ? 440 : 350 + scaleY * (user.RankHistory[i] - minRank);
        //        float y1 = scaleY == float.PositiveInfinity ? 440 : 350 + scaleY * (user.RankHistory[i + 1] - minRank);
        //        if (i % common == 0 || i + 1 == user.RankHistory.Length - 1)
        //        {
        //            int index = i + 1 == user.RankHistory.Length - 1 ? i + 1 : i;
        //            g.DrawLine(LightLinePen, 40 + scaleX * index, index != i + 1 ? y : y1, 40 + scaleX * index, 530);
        //            drawableString = $"#{user.RankHistory[index].Separate(".")}\n{user.RankHistory.Length - index - 1}d ago";
        //            x = 40 + scaleX * index - g.MeasureString(drawableString, RubikLightBold10).Width / 2;
        //            g.DrawString(drawableString, RubikLightBold10, LightGrayBrush, x, 532);
        //        }

        //        PointF start = new(40 + scaleX * i, y);
        //        PointF end = new(40 + scaleX * (i + 1), y1);
        //        g.DrawLine(GraphicPen, start, end);
        //    }
        //    #endregion

        //    return result;
        //}

        public async Task<SKImage> CreateScoresCard(IEnumerable<ScoreInfo> scores)
        {
            int width = 1000;
            int height = 136 + scores.Count() * 114;

            SKImageInfo imageInfo = new(width, height);
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                SKCanvas canvas = surface.Canvas;

                User user = scores.First().User;

                byte[] data = await _webClient.DownloadDataTaskAsync(user.AvatarUrl);

                _paint.SetColor(_backgroundSemilightColor);
                canvas.DrawRect(0, 0, width, 136, _paint);

                SKImage image = SKImage.FromEncodedData(data);
                SKRect sourceRect = new(0, 0, image.Width, image.Height);
                SKRect destRect = new() { Location = new(4, 4), Size = new(128, 128) };
                canvas.DrawImage(image, sourceRect, destRect, _paint);

                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(20);
                canvas.DrawText($"#{user.Name}", 156, 30, _paint);
                canvas.DrawText($"#{user.WorldRating}", 156, 60, _paint);
                canvas.DrawText($"({user.CountryCode} #{user.CountryRating})", 156, 90, _paint);

                int i = 0;
                foreach (ScoreInfo score in scores)
                {
                    float y = 136 + i * 114;
                    SKImage scoreImage = await CreateSmallCardAsync(score, false);
                    canvas.DrawImage(scoreImage, 0, y);
                    if (i != 0)
                    {
                        _paint.SetColor(_lightGrayColor);
                        canvas.DrawLine(0, y, width, y, _paint);
                    }
                    i++;
                }

                return surface.Snapshot();
            }
        }

        //public static Image CreateTableScoresCard(IEnumerable<ScoreInfo> scores)
        //{
        //    scores = scores.OrderByDescending(score => score.Score);

        //    int width = 1080;
        //    int height = 370 + scores.Count() * 35;

        //    Image result = new Bitmap(width, height);
        //    var g = Graphics.FromImage(result);
        //    g.SmoothingMode = SmoothingMode.AntiAlias;
        //    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        //    g.FillRectangle(BackgroundSemilightBrush, 0, 0, width, height);

        //    var beatmap = scores.First().Beatmap;
        //    using var backgroundImgStream = new MemoryStream(WebClient.DownloadData(beatmap.CoverUrl));
        //    var backgroundImage = Image.FromStream(backgroundImgStream).Darkening(128);
        //    g.DrawImage(backgroundImage, 0, 0, 1080, 300);

        //    #region map
        //    var drawableString = $"{beatmap.Title} - {beatmap.Artist} [{beatmap.DifficultyName}]";
        //    var x = width / 2 - g.MeasureString(drawableString, Rubik17).Width / 2;
        //    g.DrawString(drawableString, Rubik17, WhiteBrush, x, 10);

        //    drawableString = $"Mapped by {beatmap.MapperName}";
        //    x = width / 2 - g.MeasureString(drawableString, Rubik15).Width / 2;
        //    g.DrawString(drawableString, Rubik15, WhiteBrush, x, 40);

        //    x = 200;
        //    var y = 275;
        //    g.DrawString("CS:", Rubik15, WhiteBrush, x, y);
        //    x += g.MeasureString("CS:", Rubik15).Width;
        //    drawableString = beatmap.Attributes.CS.ToString("0.0");
        //    g.DrawString(drawableString, RubikBold15, WhiteBrush, x, y);
        //    x = x + 5 + g.MeasureString(drawableString, RubikBold15).Width;

        //    g.DrawString("AR:", Rubik15, WhiteBrush, x, y);
        //    x += g.MeasureString("AR:", Rubik15).Width;
        //    drawableString = beatmap.Attributes.AR.ToString("0.0");
        //    g.DrawString(drawableString, RubikBold15, WhiteBrush, x, y);
        //    x = x + 5 + g.MeasureString(drawableString, RubikBold15).Width;

        //    g.DrawString("OD:", Rubik15, WhiteBrush, x, y);
        //    x += g.MeasureString("OD:", Rubik15).Width;
        //    drawableString = beatmap.Attributes.OD.ToString("0.0");
        //    g.DrawString(drawableString, RubikBold15, WhiteBrush, x, y);
        //    x = x + 5 + g.MeasureString(drawableString, RubikBold15).Width;

        //    g.DrawString("HP:", Rubik15, WhiteBrush, x, y);
        //    x += g.MeasureString("HP:", Rubik15).Width;
        //    drawableString = beatmap.Attributes.HP.ToString("0.0");
        //    g.DrawString(drawableString, RubikBold15, WhiteBrush, x, y);
        //    x = x + 5 + g.MeasureString(drawableString, RubikBold15).Width;

        //    g.DrawString("Length:", Rubik15, WhiteBrush, x, y);
        //    x += g.MeasureString("Length:", Rubik15).Width;
        //    drawableString = TimeSpan.FromSeconds(beatmap.Attributes.Length).ToString(@"mm\:ss");
        //    g.DrawString(drawableString, RubikBold15, WhiteBrush, x, y);
        //    x = x + 5 + g.MeasureString(drawableString, RubikBold15).Width;

        //    g.DrawString("BPM:", Rubik15, WhiteBrush, x, y);
        //    x += g.MeasureString("BPM:", Rubik15).Width;
        //    g.DrawString(beatmap.Attributes.BPM.ToString(), RubikBold15, WhiteBrush, x, y);
        //    x = x + 5 + g.MeasureString(beatmap.Attributes.BPM.ToString(), RubikBold15).Width;

        //    g.DrawString("Stars:", Rubik15, WhiteBrush, x, y);
        //    x += g.MeasureString("Stars:", Rubik15).Width;
        //    g.DrawString($"{beatmap.Attributes.Stars:0.00} ★", RubikBold15, WhiteBrush, x, y);
        //    #endregion

        //    #region header row
        //    var centerX = 40;
        //    y = 320;
        //    drawableString = "Rank";
        //    var renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "Username";
        //    centerX += 100;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "Score";
        //    centerX += 120;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "Accuracy";
        //    centerX += 120;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "Combo";
        //    centerX += 110;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "Mods";
        //    centerX += 110;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "PP";
        //    centerX += 100;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "Date";
        //    centerX += 100;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "300";
        //    centerX += 100;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "100";
        //    centerX += 50;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "50";
        //    centerX += 50;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    drawableString = "X";
        //    centerX += 50;
        //    renderX = centerX - g.MeasureString(drawableString, RubikBold14).Width / 2;
        //    g.DrawString(drawableString, RubikBold14, LightGrayBrush, renderX, y);

        //    g.DrawLine(LightLinePen, 0, 345, width, 345);
        //    #endregion

        //    #region score rows
        //    int i = 0;
        //    foreach (var score in scores)
        //    {
        //        centerX = 40;
        //        y = 365 + 35 * i;
        //        i++;
        //        drawableString = $"# {i}";
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, WhiteBrush, renderX, y);

        //        drawableString = score.User.Name;
        //        centerX += 100;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, WhiteBrush, renderX, y);

        //        drawableString = score.Score.Separate(".");
        //        centerX += 120;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, WhiteBrush, renderX, y);

        //        var scoreString = score.Rank.Last() == 'H' ? score.Rank[..^1] : score.Rank;
        //        drawableString = $"{score.Accuracy:0.00}% ({score.Rank})";
        //        centerX += 120;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString($"{score.Accuracy:0.00}% (", Rubik13, WhiteBrush, renderX, y);

        //        renderX += g.MeasureString($"{score.Accuracy:0.00}% (", Rubik13).Width - 7;
        //        g.DrawString(scoreString, RubikBold13, RankBrushes[score.Rank], renderX, y);

        //        renderX += g.MeasureString(scoreString, RubikBold13).Width - 7;
        //        g.DrawString(")", Rubik13, WhiteBrush, renderX, y);

        //        drawableString = $"{score.MaxCombo}/{score.Beatmap.Attributes.MaxCombo}x";
        //        centerX += 110;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, WhiteBrush, renderX, y);

        //        var modsImage = ModsConverter.ToImage(score.Mods);
        //        centerX += 110;
        //        if (modsImage != null)
        //        {
        //            var modsWidth = modsImage.Width * 0.625f;
        //            var modsHeight = modsImage.Height * 0.625f;
        //            renderX = centerX - modsWidth / 2;
        //            g.DrawImage(modsImage, renderX, y, modsWidth, modsHeight);
        //        }

        //        drawableString = $"{(int)(score.PP ?? PerfomanceCalculator.Calculate(score))}pp";
        //        centerX += 100;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, WhiteBrush, renderX, y);

        //        drawableString = score.Date.ToShortDateString();
        //        centerX += 100;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, WhiteBrush, renderX, y);

        //        drawableString = score.Count300.ToString();
        //        centerX += 100;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, Brush300, renderX, y);

        //        drawableString = score.Count100.ToString();
        //        centerX += 50;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, Brush100, renderX, y);

        //        drawableString = score.Count50.ToString();
        //        centerX += 50;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, Brush50, renderX, y);

        //        drawableString = score.CountMisses.ToString();
        //        centerX += 50;
        //        renderX = centerX - g.MeasureString(drawableString, Rubik13).Width / 2;
        //        g.DrawString(drawableString, Rubik13, BrushMisses, renderX, y);
        //    }
        //    #endregion

        //    return result;
        //}
    }
}
