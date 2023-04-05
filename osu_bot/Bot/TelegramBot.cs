// Licensed to the .NET Foundation under one or more agreements.
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
        public const int REPLAYS_THREAD_ID = 1007;
        public const int REQUESTS_THREAD_ID = 1009;
        public const int BEATMAPS_THREAD_ID = 1011;
        public const int SCORES_THREAD_ID = 1024;

        private readonly ReceiverOptions _receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        #region Chats settings
#if DEBUG
        public readonly ChatId ChatId = new(-1001888790264);
        public readonly ITelegramBotClient BotClient = new TelegramBotClient("6287803710:AAFgsXlWVeh2QOtvsBymmnG87bNDXX7XqTg");
#else
        public readonly ChatId ChatId = new(-1001238663722);
        //public readonly int ReplaysThreadId = 1007;
        //public readonly int RequestsThreadId = 1009;
        //public readonly int BeatmapsThreadId = 1011;
        public readonly ITelegramBotClient BotClient = new TelegramBotClient("5701573101:AAESrGE-4nLNjqXTcWHvnQcBDkQG0pgP2IE");
#endif
        #endregion


        private readonly CallbacksManager _callbacksManager = new();
        private readonly CommandsManager _commandsManager = new();

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

            Stream botStatusStream = ResourcesManager.BotStatusManager.Online.Encode().AsStream();
            await BotClient.SetChatPhotoAsync(ChatId, botStatusStream);
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
            botStatusStream = ResourcesManager.BotStatusManager.Offline.Encode().AsStream();
            await BotClient.SetChatPhotoAsync(ChatId, botStatusStream);
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

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            string ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            BotClient.SendTextMessageAsync(
                chatId: ChatId,
                text: "Произошла ошибка (возможно превышен лимит запросов), бот перезапустится в течении 2-х секунд...");

            Task.Delay(5000).Wait();

            BotClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: cancellationToken
            );

            BotClient.SendTextMessageAsync(
                chatId: ChatId,
                text: "Бот перезапущен и готов к работе.");

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
