using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using osu_bot.Bot.Commands;
using osu_bot.Bot.Callbacks;
using osu_bot.Bot.Commands.Main;
using osu_bot.API;
using System.Linq.Expressions;
using LiteDB;
using osu_bot.Entites.Database;

namespace osu_bot.Bot
{
    public class BotHandle
    {
        private static readonly string connectionString = @"Database.db";

        private readonly Command[] commands =
        {
            new HelpCommand(),
            new StartCommand(),
            new TopCommand(),
            new LastCommand(),
            new RegCommand(),
            new StatsCommand()
        };
        private readonly Callback[] callbacks =
        {
            new HelpCallback(),
            new MapsCallback()
        };

        private readonly ITelegramBotClient botClient = new TelegramBotClient("5701573101:AAESrGE-4nLNjqXTcWHvnQcBDkQG0pgP2IE");

        private readonly Dictionary<string, Func<ITelegramBotClient, Update, CancellationToken, Task>> Commands = new();
        private readonly Dictionary<string, Func<ITelegramBotClient, Update, CancellationToken, Task>> Callbacks = new();

        private readonly DatabaseContext database = new(connectionString);

        private readonly OsuAPI API = new();

        public BotHandle() 
        {
            foreach (var command in commands)
            {
                Commands.Add(command.Text, command.ActionAsync);
                command.API = API;
                command.Database = database;
            }

            foreach (var callback in callbacks)
                Callbacks.Add(callback.Data, callback.Action);
        }

        public async Task Run()
        {
            await API.InitalizeAsync();
            using CancellationTokenSource cts = new();
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            Console.WriteLine($"Start listening...");
            Console.ReadLine();

            cts.Cancel();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.CallbackQuery != null && update.CallbackQuery.Data is { } data)
                {
                    var callback = Callbacks.Keys.FirstOrDefault(s => data.Contains(s));
                    if (callback != null)
                        await Callbacks[callback].Invoke(botClient, update, cancellationToken);
                }
                else if (update.Message != null && update.Message.Text is { } messageText)
                {
                    messageText = messageText.IndexOf(' ') != -1 
                        ? messageText[..messageText.IndexOf(' ')]
                        : messageText;
                    if (Commands.ContainsKey(messageText))
                    {
                        await Commands[messageText].Invoke(botClient, update, cancellationToken);
                    }
                }
            }
            catch(Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: ex.Message,
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);
            }
            return; 
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
