// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Modules.OsuFiles
{
    public class OsuBinaryReader : BinaryReader
    {
        public OsuBinaryReader(Stream input) : base(input)
        {

        }

        public OsuBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {

        }

        public override string ReadString()
        {
            if (ReadByte() == 0x0b)
                return base.ReadString();
            else
                return "";
        }
    }
}
