// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Exceptions
{
    public class UserRequestsNotFoundException : Exception
    {
        public string Username { get; set; }

        public override string Message => $"Реквестов для пользователя {Username} не найдено";
    }
}
