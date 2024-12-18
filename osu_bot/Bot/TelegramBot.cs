﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using LiteDB;
using osu_bot.API;
using osu_bot.Bot.Callbacks;
using osu_bot.Bot.Commands;
using osu_bot.Bot.Scanners;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using osu_bot.Resources;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot
{
    public class TelegramBot
    {
#if DEBUG
        public const int REPLAYS_THREAD_ID = 1007;
        public const int REQUESTS_THREAD_ID = 1009;
        public const int BEATMAPS_THREAD_ID = 1011;
        public const int SCORES_THREAD_ID = 1024;
        public const long CHAT_ID = -1001888790264;

        public readonly ChatId ChatId = new(CHAT_ID);
        public readonly ITelegramBotClient BotClient = new TelegramBotClient("6287803710:AAFgsXlWVeh2QOtvsBymmnG87bNDXX7XqTg");
#else
        public const int REPLAYS_THREAD_ID = 799;
        public const int REQUESTS_THREAD_ID = 801;
        public const int BEATMAPS_THREAD_ID = 805;
        public const int SCORES_THREAD_ID = 1132;
        public const long CHAT_ID = -1001238663722;

        public readonly ChatId ChatId = new(CHAT_ID);
        public readonly ITelegramBotClient BotClient = new TelegramBotClient("5701573101:AAESrGE-4nLNjqXTcWHvnQcBDkQG0pgP2IE");
#endif

        private readonly ReceiverOptions _receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        private readonly CallbacksManager _callbacksManager = new();
        private readonly CommandsManager _commandsManager = new();
        private readonly DocumentsManager _documentsManager = new();
        private readonly WebAppsManager _webAppsManager = new();

        private readonly List<Scanner> _scanners = new()
        {
            new PlaysScanner()
        };

        public async Task RunAsync()
        {
            await OsuService.Instance.InitalizeAsync(this);
            using CancellationTokenSource cts = new();

#if !DEBUG
            Console.WriteLine("Update chat photo...");
            await BotClient.SetChatPhotoAsync(ChatId, ResourcesManager.BotStatusManager.Online.ToInputFile());
#endif
            Console.WriteLine("Start listening...");

            BotClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: cts.Token
            );

            foreach (Scanner scanner in _scanners)
                scanner.Run();
            

            Console.ReadLine();
#if !DEBUG
            Console.WriteLine("Update chat photo...");
            await BotClient.SetChatPhotoAsync(ChatId, ResourcesManager.BotStatusManager.Offline.ToInputFile());
#endif
            cts.Cancel();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                    await _commandsManager.HandlingAsync(botClient, message, cancellationToken);

                    if (update.Message.Document != null)
                    {
                        await _documentsManager.HandlingAsync(botClient, message, cancellationToken);
                    }
                    if (update.Message.WebAppData != null)
                    {
                        await _webAppsManager.HandlingAsync(botClient, message, cancellationToken);
                    }
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

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            string ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            Task.Delay(20000).Wait();
            Environment.Exit(1);
            return Task.CompletedTask;
        }
    }
}
