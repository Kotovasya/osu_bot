using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.Json.Nodes;
using osu_bot.Entites;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using SkiaSharp;
using Telegram.Bot.Types;
using osu_bot.Assets;

namespace osu_bot.Modules
{
    public static class Extensions
    {
        public static byte[] ToStream(this Image image)
        {
            return new byte[1];
        }

        public static string Separate(this int number, string separator)
        {
            NumberFormatInfo format = new()
            {
                NumberGroupSeparator = separator
            };
            return number.ToString("N", format)[..^3];
        }

        public static Image Darkening(this Image image, int alpha)
        {
            Rectangle r = new Rectangle(0, 0, image.Width, image.Height);
            using Graphics g = Graphics.FromImage(image);
            using Brush cloud_brush = new SolidBrush(Color.FromArgb(alpha, Color.Black));
            g.FillRectangle(cloud_brush, r);
            return image;
        }

        public static double CalculateAccuracyFromHits(int count300, int count100, int count50, int countMiss)
        {
            return (300.0 * count300 + 100.0 * count100 + 50.0 * count50) / (300.0 * (count300 + count100 + count50 + countMiss));
        }

        public static (int, int) CalculateHitsFromAccuracy(double accuracy, int totalObjects)
        {
            int count300 = totalObjects;
            int count100 = 0;
            double lastAccuracy = 1;
            double nowAccuracy = 1;
            while (!(Math.Round(nowAccuracy, 4) <= Math.Round(accuracy, 4) && Math.Round(accuracy, 4) >= Math.Round(lastAccuracy, 4)))
            {
                count300--;
                count100++;
                lastAccuracy = nowAccuracy;
                nowAccuracy = CalculateAccuracyFromHits(count300 - 1, count100 + 1, 0, 0);
            }
            return (count300, count100);
        }

        public static IEnumerable<string> Split(this string str, int n)
        {
            if (string.IsNullOrEmpty(str) || n < 1)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < str.Length; i += n)
            {
                yield return str.Substring(i, Math.Min(n, str.Length - i));
            }
        }

        public static SKPaint SetColor(this SKPaint paint, SKColor color)
        {
            paint.Color = color;
            return paint;
        }

        public static SKPaint SetSize(this SKPaint paint, float size)
        {
            paint.TextSize = size;
            return paint;
        }

        public static SKPaint SetTypeface(this SKPaint paint, SKTypeface typeface)
        {
            paint.Typeface = typeface;
            return paint;
        }

        public static void DrawText(this SKCanvas canvas, string text, SKRect rect, SKPaint paint, bool multiLine = false)
        {
            float spaceWidth = paint.MeasureText(" ");
            float wordX = rect.Left;
            float wordY = rect.Top;
            foreach (string word in text.Split(' '))
            {
                float wordWidth = paint.MeasureText(word);
                if (wordWidth <= rect.Right - wordX)
                {
                    canvas.DrawText(word, wordX, wordY, paint);
                    wordX += wordWidth + spaceWidth;
                }
                else if (multiLine)
                {
                    wordY += paint.FontSpacing;
                    wordX = rect.Left;
                }
                else
                {
                    int i = 0;
                    string subWord;
                    do
                        subWord = $"{word[..^++i]}...";
                    while (wordWidth < rect.Right - wordX || i < word.Length);

                    canvas.DrawText(subWord, wordX, wordY, paint);
                    return;
                }
            }
        }

        public static void DrawAlignText(this SKCanvas canvas, string drawableString, float centerX, float y, SKTextAlign align, SKPaint paint)
        {
            float x = align switch
            {
                SKTextAlign.Left => centerX,
                SKTextAlign.Center => centerX - paint.MeasureText(drawableString) / 2,
                SKTextAlign.Right => centerX + paint.MeasureText(drawableString) / 2,
                _ => centerX,
            };
            canvas.DrawText(drawableString, x, y, paint);
        }
    }
}
