﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Parameters
{
    public interface IQueryParameters
    {
        public abstract string GetQueryString();
    }
}