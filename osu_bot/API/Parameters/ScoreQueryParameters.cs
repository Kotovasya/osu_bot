// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Parameters
{
    public class ScoreQueryParameters : IQueryParameters
    {
        public long ScoreId { get; set; }
        public string Username { get; set; }
        public string GetQueryString() => $"https://osu.ppy.sh/api/v2/scores/osu/{ScoreId}";
    }
}
