﻿// Licensed to the .NET Foundation under one or more agreements.
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

        #region Chats settings
#if DEBUG
        private readonly ChatId _chatId = new(-1001888790264);
        private readonly ITelegramBotClient _botClient = new TelegramBotClient("6287803710:AAFgsXlWVeh2QOtvsBymmnG87bNDXX7XqTg");
#else
        private readonly ChatId _chatId = new(-1001238663722);
        private readonly ITelegramBotClient _botClient = new TelegramBotClient("5701573101:AAESrGE-4nLNjqXTcWHvnQcBDkQG0pgP2IE");
#endif
        #endregion

        private readonly List<Parser> _parsers = new()
        {
            new PlaysParser(),
        };

        private readonly CallbacksManager _callbacksManager = new();
        private readonly CommandsManager _commandsManager = new();

        public async Task Run()
        {
            await OsuAPI.Instance.InitalizeAsync();
            using CancellationTokenSource cts = new();

            foreach (Parser parser in _parsers)
                parser.Run();

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
                if (update.CallbackQuery != null)
                {
                    message = update.CallbackQuery.Message;
                    await _callbacksManager.HandlingAsync(botClient, update.CallbackQuery, cancellationToken);                 
                }

                else if (update.Message != null)
                {
                    message = update.Message;
                    await _commandsManager.HandlingAsync(botClient, update.Message, cancellationToken);        
                }
            }
            catch (Exception ex)
            {
                if (message is not null)
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: ex.Message,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
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
