using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace osu_bot.Assets
{
    public abstract class ResourceManager
    {
        protected abstract string ResourcesPath { get; }

        public ResourceManager()
        {
            Type type = GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                string filePath = $"{ResourcesPath}\\{property.Name}.png";
                FileStream fileStream = File.Open(filePath, FileMode.Open);
                SKImage image = SKImage.FromEncodedData(fileStream);
                fileStream.Close();
                property.SetValue(this, image);
            }
        }
    }
}
