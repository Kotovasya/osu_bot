// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace osu_bot.Modules.Converters
{
    public class JsonPathConverter : JsonConverter
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            object? targetObj = existingValue ?? Activator.CreateInstance(objectType);

            foreach (PropertyInfo? prop in objectType.GetProperties().Where(p => p.CanRead))
            {
                JsonPropertyAttribute? pathAttribute = prop.GetCustomAttributes(true).OfType<JsonPropertyAttribute>().FirstOrDefault();
                JsonConverterAttribute? converterAttribute = prop.GetCustomAttributes(true).OfType<JsonConverterAttribute>().FirstOrDefault();

                string jsonPath = pathAttribute?.PropertyName ?? prop.Name;
                JToken token = jo.SelectToken(jsonPath);

                if (token != null && token.Type != JTokenType.Null)
                {
                    bool done = false;

                    if (converterAttribute != null)
                    {
                        object[] args = converterAttribute.ConverterParameters ?? Array.Empty<object>();
                        JsonConverter? converter = Activator.CreateInstance(converterAttribute.ConverterType, args) as JsonConverter;
                        if (converter != null && converter.CanRead)
                        {
                            using (StringReader sr = new(token.ToString()))
                            using (JsonTextReader jr = new(sr))
                            {
                                object value = converter.ReadJson(jr, prop.PropertyType, prop.GetValue(targetObj), serializer);
                                if (prop.CanWrite)
                                {
                                    prop.SetValue(targetObj, value);
                                }
                                done = true;
                            }
                        }
                    }

                    if (!done)
                    {
                        if (prop.CanWrite)
                        {
                            object value = token.ToObject(prop.PropertyType, serializer);
                            prop.SetValue(targetObj, value);
                        }
                        else
                        {
                            using (StringReader sr = new(token.ToString()))
                            {
                                serializer.Populate(sr, prop.GetValue(targetObj));
                            }
                        }
                    }
                }
            }

            return targetObj;
        }

        public override bool CanConvert(Type objectType) => false;

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
