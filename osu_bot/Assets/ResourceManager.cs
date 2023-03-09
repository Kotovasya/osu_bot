// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using SkiaSharp;

namespace osu_bot.Assets
{
    public abstract class ResourceManager
    {
        protected abstract string ResourcesPath { get; }

        protected abstract string FileFormat { get; }

        protected abstract object ConvertFile(FileStream stream);

        public ResourceManager()
        {
            Type type = GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                string filePath = $"{ResourcesPath}\\{property.Name}.{FileFormat}";
                FileStream fileStream = File.Open(filePath, FileMode.Open);
                object result = ConvertFile(fileStream);
                fileStream.Close();
                property.SetValue(this, result);
            }
        }
    }
}
