// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using LiteDB;
using osu_bot.Resources;
using osu_bot.Entites;
using SkiaSharp;

namespace osu_bot.Modules
{
    public class ImageGenerator
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
                    float centerX = X + (FirstPaint.MeasureText(FirstString) / 2);
                    float X2 = centerX - (SecondPaint.MeasureText(SecondString) / 2);
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

        public static ImageGenerator Instance { get; } = new();

        private readonly SKTypeface _starTypeface = SKFontManager.Default.MatchCharacter(STAR_UNICODE);
        private readonly SKTypeface _triangleTypeface = SKFontManager.Default.MatchCharacter(TRIANGLE_UNICODE);

        private readonly SKImageFilter _imageDarkingFilter = SKImageFilter.CreateColorFilter(
            SKColorFilter.CreateBlendMode(new SKColor(0, 0, 0, 140), SKBlendMode.Darken));

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
        private readonly SKColor _blackColor = new(0, 0, 0);
        private readonly SKColor _orangeColor = new(255, 204, 34);

        private readonly Dictionary<string, SKColor> _rankColors = new()
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


        private readonly SKTypeface _secularOneTypeface = ResourcesManager.FontsManager.SecularOne;

        private readonly SKTypeface _rubikTypeface = ResourcesManager.FontsManager.Rubik;

        private readonly SKTypeface _rubikLightTypeface = ResourcesManager.FontsManager.RubikLight;

        private readonly SKTypeface _rubikMediumTypeface = ResourcesManager.FontsManager.RubikMedium;

        #endregion

        private readonly Dictionary<string, SKImage> _rankStatus = new()
        {
            { "graveyard", ResourcesManager.MapStatusManager.Graveyard },
            { "wip", ResourcesManager.MapStatusManager.Graveyard },
            { "pending", ResourcesManager.MapStatusManager.Graveyard },
            { "ranked", ResourcesManager.MapStatusManager.Rating },
            { "approved", ResourcesManager.MapStatusManager.Approved },
            { "qualified", ResourcesManager.MapStatusManager.Approved },
            { "loved", ResourcesManager.MapStatusManager.Loved },
        };

        private readonly HttpClient _httpClient = new();

        private static string GetPlayedTimeString(DateTime date)
        {
            TimeSpan diff = DateTime.Now - date;
            if (diff.Days > 0)
            {
                return diff.Days <= 7 ? diff.Days == 1 ? "1 day ago" : $"{diff.Days} days ago" : date.ToString("dd MMMM yyyy г.");
            }

            if (diff.Hours > 0)
            {
                return diff.Hours == 1 ? "1 hour ago" : $"{diff.Hours} hours ago";
            }

            return diff.Minutes > 0
                ? diff.Minutes == 1 ? "1 minute ago" : $"{diff.Minutes} minutes ago"
                : diff.Seconds > 30 ? $"{diff.Seconds} seconds ago" : "few seconds ago";
        }

        public async Task<SKImage> CreateSmallCardAsync(OsuScoreInfo score, bool showNick)
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

                
                byte[] data = await _httpClient.GetByteArrayAsync(score.Beatmap.CoverUrl);
                SKImage image = SKImage.FromEncodedData(data);
                SKRect sourceRect = new() { Location = new SKPoint((image.Width / 2) - 406, (image.Height / 2) - 250), Size = new SKSize(812, 500) };
                SKRect destRect = new() { Location = new SKPoint(x, 14), Size = new SKSize(146, 90) };
                canvas.DrawImage(image, sourceRect, destRect, _paint);

                string drawableString = score.Rank.Last() == 'H' ? score.Rank[..^1] : score.Rank;

                _paint.SetColor(_rankColors[score.Rank]).SetTypeface(_secularOneTypeface).SetSize(64);

                x = (x / 2) - (_paint.MeasureText(drawableString) / 2);
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

                if (score.Rank ==  "F")
                {
                    float hits = score.HitObjects * 1.0f / score.Beatmap.Attributes.TotalObjects * 100.0f;
                    drawableString = $"{hits:F2}%";
                    _paint.SetColor(_whiteColor);
                    x = 50 - (_paint.MeasureText(drawableString) / 2);
                    canvas.DrawText(drawableString, x, y, _paint);
                }

                int pp = score.PP != null ? (int)score.PP : PerfomanceCalculator.Calculate(score);
                drawableString = $"{pp} PP";
                _paint.SetColor(_whiteColor).SetSize(30);
                x = width - 15 - _paint.MeasureText(drawableString);
                canvas.DrawText(drawableString, x, 40, _paint);

                SKImage? modsImage = ModsConverter.ToImage(score.Mods);
                if (modsImage != null)
                {
                    canvas.DrawImage(modsImage, width - 15 - modsImage.Width, 59);
                }
                #endregion

                return surface.Snapshot();
            }
        }

        public async Task<SKImage> CreateFullCardAsync(OsuScoreInfo score)
        {
            int width = 1080;
            int height = 376;

            SKImageInfo imageInfo = new(width, height);
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                SKCanvas canvas = surface.Canvas;

                canvas.Clear(_backgroundColor);

                #region Background map image
                byte[] data = await _httpClient.GetByteArrayAsync(score.Beatmap.CoverUrl);
                SKRect sourceRect = new() { Location = new SKPoint(0, 36), Size = new SKSize(1800, 428) };
                SKRect destRect = new() { Location = new SKPoint(204, 0), Size = new SKSize(876, 204) };
                SKImage image = SKImage.FromEncodedData(data);
                SKRectI imageSize = new(0, 0, image.Width, image.Height);
                image = image.ApplyImageFilter(_imageDarkingFilter, imageSize, imageSize, out _, out SKPointI _);
                canvas.DrawImage(image, sourceRect, destRect, _paint);
                #endregion

                #region Avatar image
                data = await _httpClient.GetByteArrayAsync(score.User.AvatarUrl);
                image = SKImage.FromEncodedData(data);
                imageSize = new SKRectI(0, 0, image.Width, image.Height);
                image = image.ApplyImageFilter(_imageDarkingFilter, imageSize, imageSize, out _, out SKPointI _);
                destRect = new SKRect() { Location = new SKPoint(0, 0), Size = new SKSize(204, 204) };
                canvas.DrawImage(image, imageSize, destRect, _paint);

                SKPaint paint1 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(18);
                SKPaint paint2 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikLightTypeface).SetSize(18);
                StringsLinker stringLinker = new(paint1, paint2, true);

                string drawableString = $"({score.User.CountryCode} #{score.User.CountryRating})";
                float x = 102 - (paint1.MeasureText($"#{score.User.WorldRating}") / 2);

                stringLinker.SetStrings($"#{score.User.WorldRating}", drawableString)
                    .SetPositions(x, 23, 48)
                    .Draw(canvas);

                drawableString = $"{score.User.PP.Separate(".")}pp";
                x = 102 - (paint1.MeasureText(score.User.Name) / 2);

                stringLinker.SetStrings(score.User.Name, drawableString)
                    .SetPositions(x, 173, 198)
                    .Draw(canvas);
                #endregion

                #region Map info
                x = 209;
                SKImage mapStatusImage = _rankStatus[score.Beatmap.Status];
                canvas.DrawImage(mapStatusImage, x, 5, _paint);

                x += mapStatusImage.Width + 5;

                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(20);
                drawableString = $"{score.Beatmap.Title} [{score.Beatmap.DifficultyName}]";
                canvas.DrawText(drawableString, x, 23, _paint);

                drawableString = $"{score.Beatmap.Artist}";
                _paint.SetSize(18);
                canvas.DrawText(drawableString, x, 48, _paint);

                x = 213;
                drawableString = $"Mapped by {score.Beatmap.MapperName}";
                _paint.SetSize(15);
                canvas.DrawText(drawableString, x, 73, _paint);

                float y = 195;
                float columnSpacing = 18;
                float wordSpacing = 4;
                paint1 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikLightTypeface).SetSize(18);
                paint2 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(18);
                stringLinker = new StringsLinker(paint1, paint2, wordSpacing);

                drawableString = score.Beatmap.Attributes.BaseCS.ToString("0.0");
                x = stringLinker.SetStrings("CS:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                if (score.Beatmap.Attributes.CS != score.Beatmap.Attributes.BaseCS)
                {
                    x += 2;
                    drawableString = $"{score.Beatmap.Attributes.CS:0.0}";
                    _paint.SetColor(_colorMisses).SetTypeface(_rubikMediumTypeface).SetSize(16);
                    canvas.DrawText(drawableString, x, y - 6, _paint);

                    x += _paint.MeasureText(drawableString);
                    drawableString = "▲";
                    _paint.SetTypeface(_triangleTypeface);
                    canvas.DrawText(drawableString, x, y - 6, _paint);
                    x += _paint.MeasureText(drawableString);
                }

                x += columnSpacing;

                drawableString = score.Beatmap.Attributes.BaseAR.ToString("0.0");
                x = stringLinker.SetStrings("AR:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                if (score.Beatmap.Attributes.AR != score.Beatmap.Attributes.BaseAR)
                {
                    x += 2;
                    drawableString = $"{score.Beatmap.Attributes.AR:0.0}";
                    _paint.SetColor(_colorMisses).SetTypeface(_rubikMediumTypeface).SetSize(16);
                    canvas.DrawText(drawableString, x, y - 6, _paint);

                    x += _paint.MeasureText(drawableString);
                    drawableString = "▲";
                    _paint.SetTypeface(_triangleTypeface);
                    canvas.DrawText(drawableString, x, y - 6, _paint);
                    x += _paint.MeasureText(drawableString);
                }

                x += columnSpacing;

                drawableString = score.Beatmap.Attributes.BaseOD.ToString("0.0");
                x = stringLinker.SetStrings("OD:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                if (score.Beatmap.Attributes.OD != score.Beatmap.Attributes.BaseOD)
                {
                    x += 2;
                    drawableString = $"{score.Beatmap.Attributes.OD:0.0}";
                    _paint.SetColor(_colorMisses).SetTypeface(_rubikMediumTypeface).SetSize(16);
                    canvas.DrawText(drawableString, x, y - 6, _paint);

                    x += _paint.MeasureText(drawableString);
                    drawableString = "▲";
                    _paint.SetTypeface(_triangleTypeface);
                    canvas.DrawText(drawableString, x, y - 6, _paint);
                    x += _paint.MeasureText(drawableString);
                }

                x += columnSpacing;

                drawableString = score.Beatmap.Attributes.BaseHP.ToString("0.0");
                x = stringLinker.SetStrings("HP:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                if (score.Beatmap.Attributes.HP != score.Beatmap.Attributes.BaseHP)
                {
                    x += 2;
                    drawableString = $"{score.Beatmap.Attributes.HP:0.0}";
                    _paint.SetColor(_colorMisses).SetTypeface(_rubikMediumTypeface).SetSize(16);
                    canvas.DrawText(drawableString, x, y - 4, _paint);

                    x += _paint.MeasureText(drawableString);
                    drawableString = "▲";
                    _paint.SetTypeface(_triangleTypeface);
                    canvas.DrawText(drawableString, x, y - 4, _paint);
                    x += _paint.MeasureText(drawableString);
                }

                x += columnSpacing;

                drawableString = TimeSpan.FromSeconds(score.Beatmap.Attributes.Length).ToString(@"mm\:ss");
                x = stringLinker.SetStrings("Length:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString = score.Beatmap.Attributes.BPM.ToString();
                x = stringLinker.SetStrings("BPM:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString = $"{score.Beatmap.Attributes.Stars:0.00}";
                x = stringLinker.SetStrings("Stars:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                drawableString = "★";
                _paint.SetColor(_whiteColor).SetTypeface(_starTypeface).SetSize(20);
                canvas.DrawText(drawableString, x, y, _paint);
                #endregion

                #region Score line 1
                x = 70;
                y = 242;
                columnSpacing = 130;

                drawableString = "Rank";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikMediumTypeface).SetSize(18);
                canvas.DrawText(drawableString, x, y, _paint);
                float stringLength = _paint.MeasureText(drawableString);

                drawableString = score.Rank.Last() == 'H' ? score.Rank[..^1] : score.Rank;
                _paint.SetColor(_rankColors[score.Rank]).SetTypeface(_secularOneTypeface).SetSize(42);
                float centerX = x + (stringLength / 2) - (_paint.MeasureText(drawableString) / 2);
                canvas.DrawText(drawableString, centerX, y + 40, _paint);

                x += columnSpacing + stringLength;

                paint1 = new SKPaint().SetColor(_lightGrayColor).SetTypeface(_rubikMediumTypeface).SetSize(18);
                paint2 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(28);
                stringLinker = new StringsLinker(paint1, paint2);

                int pp = score.PP != null ? (int)score.PP : PerfomanceCalculator.Calculate(score);
                drawableString = $"{pp}PP";
                x = stringLinker.SetStrings("Perfomance", drawableString)
                    .SetPositions(x, y, y + 30)
                    .Draw(canvas);
                x += columnSpacing;

                drawableString = $"{score.MaxCombo}x/{score.Beatmap.Attributes.MaxCombo}x";
                x = stringLinker.SetStrings("Combo", drawableString)
                    .SetPositions(x, y, y + 30)
                    .Draw(canvas);
                x += columnSpacing;

                drawableString = $"{score.Accuracy:F2}%";
                x = stringLinker.SetStrings("Accuracy", drawableString)
                    .SetPositions(x, y, y + 30)
                    .Draw(canvas);
                x += columnSpacing;

                drawableString = "Mods";
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikMediumTypeface).SetSize(18);
                canvas.DrawText(drawableString, x, y, _paint);
                stringLength = _paint.MeasureText(drawableString);

                SKImage? modsImage = ModsConverter.ToImage(score.Mods);
                if (modsImage != null)
                {
                    centerX = x + (stringLength / 2) - (modsImage.Width / 2);
                    if (centerX + modsImage.Width < width)
                    {
                        canvas.DrawImage(modsImage, centerX, 250, _paint);
                    }
                    else
                    {
                        canvas.DrawImage(modsImage, width - 5 - modsImage.Width, 250, _paint);
                    }
                }
                #endregion

                #region Score line 2
                x = 70;
                y = 335;
                columnSpacing = 60;

                paint1 = new SKPaint().SetColor(_lightGrayColor).SetTypeface(_rubikMediumTypeface).SetSize(16);
                paint2 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(16);
                stringLinker = new StringsLinker(paint1, paint2);

                drawableString = score.Score.Separate(".");
                x = stringLinker.SetStrings("Score", drawableString)
                    .SetPositions(x, y, y + 20)
                    .Draw(canvas);
                x += columnSpacing;

                float hits = score.HitObjects * 1.0f / score.Beatmap.Attributes.TotalObjects * 100.0f;
                drawableString = $"{hits:F2}%";
                x = stringLinker.SetStrings("Hit objects", drawableString)
                    .SetPositions(x, y, y + 20)
                    .Draw(canvas);
                x += columnSpacing;

                drawableString = $"{PerfomanceCalculator.Calculate(score, isFullCombo: true)}pp";
                x = stringLinker.SetStrings("For FC", drawableString)
                    .SetPositions(x, y, y + 20)
                    .Draw(canvas);
                x += columnSpacing;

                drawableString = $"{PerfomanceCalculator.Calculate(score, isFullCombo: true, isPerfect: true)}pp";
                x = stringLinker.SetStrings("For SS", drawableString)
                    .SetPositions(x, y, y + 20)
                    .Draw(canvas);
                x += columnSpacing;

                columnSpacing = 20;

                stringLinker.SecondPaint.SetColor(_color300);
                drawableString = score.Count300.ToString();
                x = stringLinker.SetStrings("300", drawableString)
                    .SetPositions(x, y, y + 20)
                    .Draw(canvas);
                x += columnSpacing;

                stringLinker.SecondPaint.SetColor(_color100);
                drawableString = score.Count100.ToString();
                x = stringLinker.SetStrings("100", drawableString)
                    .SetPositions(x, y, y + 20)
                    .Draw(canvas);
                x += columnSpacing;

                stringLinker.SecondPaint.SetColor(_color50);
                drawableString = score.Count50.ToString();
                x = stringLinker.SetStrings("50", drawableString)
                    .SetPositions(x, y, y + 20)
                    .Draw(canvas);
                x += columnSpacing;

                stringLinker.SecondPaint.SetColor(_colorMisses);
                drawableString = score.CountMisses.ToString();
                stringLinker.SetStrings("X", drawableString)
                    .SetPositions(x, y, y + 20)
                    .Draw(canvas);

                x = width - 200;
                stringLinker.SecondPaint.SetColor(_whiteColor);
                drawableString = GetPlayedTimeString(score.Date);
                stringLinker.SetStrings("Played", drawableString)
                    .SetPositions(x, y, y + 20)
                    .Draw(canvas);
                #endregion

                return surface.Snapshot();
            }
        }

        public async Task<SKImage> CreateProfileCardAsync(OsuUser user)
        {
            int width = 600;
            int height = 530;

            int avatarWidth = 286;
            int avatarHeight = 304;

            SKImageInfo imageInfo = new(width, height);
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                SKCanvas canvas = surface.Canvas;

                SKRect rect = new(0, 0, width, avatarHeight);
                _paint.SetColor(_backgroundColor);
                canvas.DrawRect(rect, _paint);

                rect = new(0, avatarHeight, width, height);
                _paint.SetColor(_backgroundSemilightColor);
                canvas.DrawRect(rect, _paint);

                #region Avatar
                byte[] data = await _httpClient.GetByteArrayAsync(user.AvatarUrl);
                SKImage image = SKImage.FromEncodedData(data);
                SKRectI imageSize = new(0, 0, image.Width, image.Height);
                SKRect destRect = new() { Location = new SKPoint(15, 35), Size = new SKSize(256, 256) };
                canvas.DrawImage(image, imageSize, destRect, _paint);

                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(22);
                string drawableString = user.Name;
                canvas.DrawAlignText(drawableString, avatarWidth / 2, 28, SKTextAlign.Center, _paint);
                #endregion

                #region Stats
                float x = 296;
                float y = 35;
                float rowSpacing = 30;
                int wordSpacing = 5;

                drawableString = $"#{user.WorldRating.Separate(".")}";
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(22);
                canvas.DrawText(drawableString, x, y, _paint);
                y += rowSpacing;

                drawableString = $"#{user.CountryRating} {user.CountryCode}";
                canvas.DrawText(drawableString, x, 65, _paint);


                SKPaint paint1 = new SKPaint().SetColor(_lightGrayColor).SetTypeface(_rubikTypeface).SetSize(18);
                SKPaint paint2 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(18);
                StringsLinker stringLinker = new(paint1, paint2, wordSpacing);

                y = 115;
                drawableString = $"{user.PP.Separate(".")}pp";
                stringLinker.SetStrings("Perfomance:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);
                y += rowSpacing;

                drawableString = $"{user.Accuracy:0.00}%";
                stringLinker.SetStrings("Accuracy:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);
                y += rowSpacing;

                drawableString = user.PlayCount.Separate(".");
                stringLinker.SetStrings("Playcount:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);
                y += rowSpacing;

                drawableString = $"{user.PlayTime.Days}d {user.PlayTime.Hours}h {user.PlayTime.Minutes}m {user.PlayTime.Seconds}s";
                stringLinker.SetStrings("Playtime:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);
                y += rowSpacing;

                drawableString = user.LastOnline != null ? GetPlayedTimeString(user.LastOnline.Value) : "скрыто";
                stringLinker.SetStrings("Online:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);
                y += rowSpacing;

                drawableString = user.DateRegistration.ToString("dd MM yyyy г.");
                stringLinker.SetStrings("Registration:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                #endregion

                #region Rank history
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(24);
                drawableString = "GLOBAL RANK HISTORY";
                canvas.DrawAlignText(drawableString, width / 2, 330, SKTextAlign.Center, _paint);

                float graphWidth = 520f;
                float graphHeight = 110f;

                int linesCount = 5;
                int coef = user.RankHistory.Length / linesCount;
                int maxRank = user.RankHistory.Max();
                int minRank = user.RankHistory.Min();
                int rankDiff = maxRank - minRank;
                float scaleX = graphWidth / user.RankHistory.Length;
                float scaleY = rankDiff != 0 ? graphHeight / rankDiff : graphHeight / 2;
                float startX = 55;
                float startY = 350;
                SKPaint linePaint = new()
                {
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                };
                using SKPath path = new();
                x = startX + (scaleX * 0);
                y = startY + (scaleY * (user.RankHistory[0] - minRank));
                path.MoveTo(x, y);
                for (int i = 0; i <= user.RankHistory.Length; i++)
                {
                    if (i < user.RankHistory.Length - 1)
                    {
                        x = startX + (scaleX * i) + 1;
                        y = startY + (scaleY * (user.RankHistory[i + 1] - minRank));
                        path.LineTo(x, y);
                    }

                    if (i % coef == 0)
                    {
                        linePaint.SetColor(_lightGrayColor)
                            .StrokeWidth = 0.4f;
                        canvas.DrawLine(x, y, x, startY + graphHeight, linePaint);
                        _paint.SetColor(_lightGrayColor).SetTypeface(_rubikLightTypeface).SetSize(15);
                        int day = user.RankHistory.Length - i;
                        drawableString = day == 0 ? "now" : $"{day} d ago";

                        float centerX = x - (_paint.MeasureText(drawableString) / 2);
                        canvas.DrawText(drawableString, centerX, startY + graphHeight + 40, _paint);

                        drawableString = i != user.RankHistory.Length ? $"#{user.RankHistory[i].Separate(".")}" : $"#{user.RankHistory[i - 1].Separate(".")}";
                        centerX = x - (_paint.MeasureText(drawableString) / 2);
                        canvas.DrawText(drawableString, centerX, startY + graphHeight + 40 + _paint.FontSpacing, _paint);
                    }

                    linePaint.SetColor(_orangeColor)
                        .StrokeWidth = 2f;
                }
                canvas.DrawPath(path, linePaint);
                path.Dispose();
                #endregion

                return surface.Snapshot();
            }
        }

        public async Task<SKImage> CreateScoresCardAsync(IEnumerable<OsuScoreInfo> scores)
        {
            int width = 1000;
            int height = 136 + (scores.Count() * 114);

            SKImageInfo imageInfo = new(width, height);
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                SKCanvas canvas = surface.Canvas;

                OsuUser user = scores.First().User;

                byte[] data = await _httpClient.GetByteArrayAsync(user.AvatarUrl);

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
                foreach (OsuScoreInfo score in scores)
                {
                    float y = 136 + (i * 114);
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

        public async Task<SKImage> CreateTableScoresCardAsync(IEnumerable<OsuScoreInfo> scores)
        {
            scores = scores.OrderByDescending(score => score.Score).Take(15);

            int width = 1080;
            int height = 370 + (scores.Count() * 35);

            SKImageInfo imageInfo = new(width, height);
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(_backgroundSemilightColor);

                #region Map info
                OsuBeatmap beatmap = scores.First().Beatmap;
                byte[] data = await _httpClient.GetByteArrayAsync(beatmap.CoverUrl);
                SKImage image = SKImage.FromEncodedData(data);
                SKRectI imageSize = new(0, 0, image.Width, image.Height);
                image = image.ApplyImageFilter(_imageDarkingFilter, imageSize, imageSize, out _, out SKPointI _);
                SKRect dest = new(0, 0, 1080, 300);
                canvas.DrawImage(image, dest, _paint);

                string drawableString = $"{beatmap.Title} - {beatmap.Artist} [{beatmap.DifficultyName}]";
                _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(20);
                canvas.DrawAlignText(drawableString, width / 2, 25, SKTextAlign.Center, _paint);

                drawableString = $"Mapped by {beatmap.MapperName}";
                _paint.SetSize(16);
                canvas.DrawAlignText(drawableString, width / 2, 45, SKTextAlign.Center, _paint);

                float x = 200;
                float y = 295;
                float columnSpacing = 18;
                float wordSpacing = 4;
                SKPaint paint1 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikLightTypeface).SetSize(18);
                SKPaint paint2 = new SKPaint().SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(18);
                StringsLinker stringLinker = new(paint1, paint2, wordSpacing);

                drawableString = beatmap.Attributes.BaseCS.ToString("0.0");
                x = stringLinker.SetStrings("CS:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString = beatmap.Attributes.BaseAR.ToString("0.0");
                x = stringLinker.SetStrings("AR:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString = beatmap.Attributes.BaseOD.ToString("0.0");
                x = stringLinker.SetStrings("OD:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString = beatmap.Attributes.BaseHP.ToString("0.0");
                x = stringLinker.SetStrings("HP:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString = TimeSpan.FromSeconds(beatmap.Attributes.Length).ToString(@"mm\:ss");
                x = stringLinker.SetStrings("Length:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString = beatmap.Attributes.BPM.ToString();
                x = stringLinker.SetStrings("BPM:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                x += columnSpacing;

                drawableString = $"{beatmap.Attributes.Stars:0.00}";
                x = stringLinker.SetStrings("Stars:", drawableString)
                    .SetPositions(x, y, y)
                    .Draw(canvas);

                drawableString = "★";
                _paint.SetColor(_whiteColor).SetTypeface(_starTypeface).SetSize(20);
                canvas.DrawText(drawableString, x, y, _paint);
                #endregion

                #region Header row
                float centerX = 40;
                y = 336;
                _paint.SetColor(_lightGrayColor).SetTypeface(_rubikMediumTypeface).SetSize(16);

                drawableString = "Rank";
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "Username";
                centerX += 100;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "Score";
                centerX += 120;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "Accuracy";
                centerX += 120;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "Combo";
                centerX += 110;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "Mods";
                centerX += 110;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "PP";
                centerX += 100;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "Date";
                centerX += 100;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "300";
                centerX += 100;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "100";
                centerX += 50;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "50";
                centerX += 50;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                drawableString = "X";
                centerX += 50;
                canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                canvas.DrawLine(0, 345, width, 345, _paint);
                #endregion

                #region Score rows
                int i = 0;
                foreach (OsuScoreInfo score in scores)
                {
                    centerX = 40;
                    y = 380 + (35 * i);
                    i++;
                    _paint.SetColor(_whiteColor).SetTypeface(_rubikTypeface).SetSize(15);
                    drawableString = $"# {i}";
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                    drawableString = score.User.Name;
                    centerX += 100;
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                    drawableString = score.Score.Separate(".");
                    centerX += 120;
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                    string rankString = score.Rank.Last() == 'H' ? score.Rank[..^1] : score.Rank;
                    drawableString = $"{score.Accuracy:0.00}% ({score.Rank})";
                    centerX += 120;
                    x = centerX - (_paint.MeasureText(drawableString) / 2);
                    canvas.DrawText($"{score.Accuracy:0.00}% (", x, y, _paint);
                    x += _paint.MeasureText($"{score.Accuracy:0.00}% (") + 1;

                    _paint.SetColor(_rankColors[rankString]);
                    canvas.DrawText(rankString, x, y, _paint);
                    x += _paint.MeasureText(rankString) + 1;

                    _paint.SetColor(_whiteColor);
                    canvas.DrawText(")", x, y, _paint);

                    drawableString = $"{score.MaxCombo}/{score.Beatmap.Attributes.MaxCombo}x";
                    centerX += 110;
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                    SKImage? modsImage = ModsConverter.ToImage(score.Mods);
                    centerX += 110;
                    if (modsImage != null)
                    {
                        float modsWidth = modsImage.Width * 0.625f;
                        float modsHeight = modsImage.Height * 0.625f;
                        x = centerX - (modsWidth / 2);
                        dest = new SKRect() { Location = new(x, y - 15), Size = new(modsWidth, modsHeight) };
                        canvas.DrawImage(modsImage, dest, _paint);
                    }

                    drawableString = $"{(int)(score.PP ?? PerfomanceCalculator.Calculate(score))}pp";
                    centerX += 100;
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                    drawableString = score.Date.ToShortDateString();
                    centerX += 100;
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                    drawableString = score.Count300.ToString();
                    centerX += 100;
                    _paint.SetColor(_color300);
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                    drawableString = score.Count100.ToString();
                    centerX += 50;
                    _paint.SetColor(_color100);
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                    drawableString = score.Count50.ToString();
                    centerX += 50;
                    _paint.SetColor(_color50);
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);

                    drawableString = score.CountMisses.ToString();
                    centerX += 50;
                    _paint.SetColor(_colorMisses);
                    canvas.DrawAlignText(drawableString, centerX, y, SKTextAlign.Center, _paint);
                }
                #endregion

                return surface.Snapshot();
            }
        }
    }
}
