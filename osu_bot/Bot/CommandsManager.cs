// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Bot.Commands;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace osu_bot.Bot
{
    public class CommandsManager
    {
        private static readonly ICommand[] s_commands =
{
            new HelpCommand(),
            new TopCommand(),
            new LastCommand(),
            new RegCommand(),
            new StatsCommand()
        };

        private readonly Dictionary<string, Func<ITelegramBotClient, Message, CancellationToken, Task>> _commands = new();

        public CommandsManager()
        {
            foreach (ICommand command in s_commands)
                _commands.Add(command.CommandText, command.ActionAsync);
        }

        public async Task HandlingAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text is { } messageText)
            {
                int spaceIndex = messageText.IndexOf(' ');
                if (spaceIndex != -1)
                    messageText = messageText[..spaceIndex];
                if (_commands.ContainsKey(messageText))
                {
                    await _commands[messageText].Invoke(botClient, message, cancellationToken);
                }
            }
        }
    }
}
