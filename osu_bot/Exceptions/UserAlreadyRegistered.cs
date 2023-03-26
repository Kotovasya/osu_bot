// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Exceptions
{
    public class UserAlreadyRegistered : Exception
    {
        public string Username { get; set; }

        public UserAlreadyRegistered(string username) => Username = username;

        public override string Message => $"Пользователь с именем {Username} уже зарегистрирован";
    }
}
