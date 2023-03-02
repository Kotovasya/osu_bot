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

namespace osu_bot.Modules
{
    public static class Extensions
    {
        public static byte[] ToStream(this Image image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }
        public static Image RoundCorners(Image StartImage, int CornerRadius, Color BackgroundColor)
        {
            CornerRadius *= 2;
            Bitmap RoundedImage = new Bitmap(StartImage.Width, StartImage.Height);
            using (Graphics g = Graphics.FromImage(RoundedImage))
            {
                g.Clear(BackgroundColor);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Brush brush = new TextureBrush(StartImage);
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(0, 0, CornerRadius, CornerRadius, 180, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0, CornerRadius, CornerRadius, 270, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                gp.AddArc(0, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
                g.FillPath(brush, gp);
                return RoundedImage;
            }
        }

        public static RectangleF RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return bounds;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path.GetBounds();
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
    }
}
