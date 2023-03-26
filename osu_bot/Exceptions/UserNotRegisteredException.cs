// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Exceptions
{
    public class UserNotRegisteredException : Exception
    {
        public override string Message => "Аккаунт Osu не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки";
    }
}
