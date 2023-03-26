// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu_bot.Modules.Converters
{
    internal class JsonModsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => throw new NotImplementedException();
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray array = JArray.Load(reader);
            return ModsConverter.ToInt(array.ToObject<IEnumerable<string>>());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;
    }
}
