// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkiaSharp;

namespace osu_bot.Entites.Database
{
    public class ReplaySkin
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("presentationName")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("lowResPreview")]
        public string PreviewImage { get; set; }
    }
}
