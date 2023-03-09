// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace osu_bot.Exceptions
{
    public class ModsArgumentException : ArgumentException
    {
        public ModsArgumentException(string? paramName = null)
            : base(paramName)
        {

        }

        public override string Message =>
            ParamName != null
            ? $"Неизвестный мод {ParamName}"
            : "Неправильно указаны моды, пример: +DTHD, +PFTDFL, +HR";
    }
}
