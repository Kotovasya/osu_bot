﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API
{
    internal interface IJsonParameters
    {
        public JObject GetJson();
    }
}