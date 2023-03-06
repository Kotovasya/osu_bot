// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            Type type = typeof(ResourceManager);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                FileStream fileStream = File.Open(ResourcesPath, FileMode.Open);
                SKImage image = SKImage.FromEncodedData(fileStream);
                fileStream.Close();
                property.SetValue(null, image);
            }
        }
    }
}
