// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Bot
{
    public class CallbackResult
    {
        public bool? ShowAlert { get; set; }

        public string? Text { get; set; }

        public int? CacheTime { get; set; }

        private CallbackResult()
        {
            ShowAlert = null;
            Text = null;
            CacheTime = null;
        }

        public CallbackResult(string? text, int? cacheTime = null)
        {
            ShowAlert = true;
            Text = text;
            CacheTime = cacheTime;
        }

        public static CallbackResult Empty()
        {
            return new CallbackResult();
        }
    }
}
