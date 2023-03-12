// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using LiteDB;
using osu_bot.API;
using osu_bot.Bot.Callbacks;
using osu_bot.Bot.Commands;
using osu_bot.Bot.Parsers;
using osu_bot.Resources;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace osu_bot.Bot
{
    public class BotHandle
    {
        private static readonly ICommand[] s_commands =
        {
            new HelpCommand(),
            new TopCommand(),
            new LastCommand(),
            new RegCommand(),
            new StatsCommand()
        };
        private static readonly Callback[] s_callbacks =
        {
            new HelpCallback(),
            new MapsCallback(),
            new MyScoreCallback(),
            new TopConferenceCallback(),
        };

#if DEBUG
        private readonly ChatId _chatId = new(-1001888790264);
        private readonly ITelegramBotClient _botClient = new TelegramBotClient("6287803710:AAFgsXlWVeh2QOtvsBymmnG87bNDXX7XqTg");
#else
        private readonly ChatId _chatId = new();
        private readonly ITelegramBotClient _botClient = new TelegramBotClient("5701573101:AAESrGE-4nLNjqXTcWHvnQcBDkQG0pgP2IE");
#endif

        private readonly Dictionary<string, Func<ITelegramBotClient, Update, CancellationToken, Task>> _commands = new();
        private readonly Dictionary<string, Func<ITelegramBotClient, Update, CancellationToken, Task>> _callbacks = new();
        private readonly List<Parser> _parsers = new()
        {
            new PlaysParser(),
        };

        public BotHandle()
        {
            foreach (ICommand command in s_commands)
                _commands.Add(command.CommandText, command.ActionAsync);

            foreach (Callback callback in s_callbacks)
                _callbacks.Add(callback.Data, callback.ActionAsync);
        }

        public async Task Run()
        {
            await OsuAPI.Instance.InitalizeAsync();
            using CancellationTokenSource cts = new();

            foreach (Parser parser in _parsers)
                await parser.RunAsync();

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            Console.WriteLine("Update chat photo...");

            Stream botStatusStream = ResourcesManager.BotStatusManager.Online.Encode().AsStream();
            await _botClient.SetChatPhotoAsync(_chatId, botStatusStream);

            Console.WriteLine("Start listening...");

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );           
            Console.ReadLine();

            Console.WriteLine("Update chat photo...");
            botStatusStream = ResourcesManager.BotStatusManager.Offline.Encode().AsStream();
            await _botClient.SetChatPhotoAsync(_chatId, botStatusStream);
            cts.Cancel();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Message? message = null;
            try
            {
                if (update.CallbackQuery != null && update.CallbackQuery.Data is { } data)
                {
                    message = update.CallbackQuery.Message;
                    string? callback = _callbacks.Keys.FirstOrDefault(s => data.Contains(s));
                    if (callback != null)
                    {
                        await _callbacks[callback].Invoke(botClient, update, cancellationToken);
                    }
                }
                else if (update.Message != null && update.Message.Text is { } messageText)
                {
                    message = update.Message;
                    messageText = messageText.IndexOf(' ') != -1
                        ? messageText[..messageText.IndexOf(' ')]
                        : messageText;
                    if (_commands.ContainsKey(messageText))
                    {
                        await _commands[messageText].Invoke(botClient, update, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                if (message != null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: ex.Message,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
                }
            }
            return;
        }

        public static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            string ErrorMessage = exception switch
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
