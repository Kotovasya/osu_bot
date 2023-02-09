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

namespace osu_bot
{
    public static class Extensions
    {
        public static Stream ToStream(this Image image)
        {
            using MemoryStream ms = new MemoryStream();
            image.Save(ms, image.RawFormat);
            ms.Position = 0;
            return ms;
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

        public static void CalculateAttributesWithMods(this BeatmapAttributes attributes, Mods mods)
        {
            float ratio;
            if (mods.HasFlag(Mods.HR))
            {
                ratio = 1.4f;
                attributes.CS = Math.Min(attributes.CS * 1.3f, 10.0f);
                attributes.AR = Math.Min(attributes.AR * ratio, 10.0f);
                attributes.OD = Math.Min(attributes.OD * ratio, 10.0f);
                attributes.HP = Math.Min(attributes.HP * ratio, 10.0f);
            }
            if (mods.HasFlag(Mods.EZ))
            {
                ratio = 0.5f;
                attributes.CS *= ratio;
                attributes.AR *= ratio;
                attributes.OD *= ratio;
                attributes.HP *= ratio;
            }
            if (mods.HasFlag(Mods.DT) || mods.HasFlag(Mods.NC))
            {
                attributes.AR = CalculateAdjustAttribute(attributes.AR, 1.5f);
                attributes.OD = CalculateAdjustAttribute(attributes.OD, 1.5f);
                attributes.Length = (int)Math.Round(attributes.Length * 0.75f);
                attributes.BPM = (int)Math.Round(attributes.BPM * 1.5f);
            }
            else if (mods.HasFlag(Mods.HT)) 
            {
                attributes.AR = CalculateAdjustAttribute(attributes.AR, 0.75f);
                attributes.OD = CalculateAdjustAttribute(attributes.OD, 0.75f);
                attributes.Length = (int)Math.Round(attributes.Length * 1.5f);
                attributes.BPM = (int)Math.Round(attributes.BPM * 0.75f);
            }
        }

        /// <summary>
        /// Method for calculate AR and OD attributes
        /// </summary>
        /// <param name="attribute">Ar or OD attribute</param>
        /// <param name="coefficient">For HalfTime = 0.75, DoubleTime = 1.5</param>
        /// <param name="mods"></param>
        /// <returns></returns>
        private static float CalculateAdjustAttribute(float attribute, float coefficient)
        {
            float ms;             
            if (attribute > 5)
                ms = 1200 + (450 - 1200) * (attribute - 5) / coefficient;
            else if (attribute < 5)
                ms = 1200 - (1200 - 1800) * (5 - attribute) / coefficient;
            else
                ms = 1200;
            
            if (ms > 1200)
                return (1800 - ms) / 120;
            else
                return (1200 - ms) / 150 + 5;
        }
    }
}
