// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Bot.Parsers
{
    public abstract class Parser
    {
        protected abstract TimeSpan Delay { get; }

        protected abstract Task ActionAsync();

        public void Run()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await ActionAsync();
                    Task.Delay(Delay).Wait();
                }
            });            
        }
    }
}
