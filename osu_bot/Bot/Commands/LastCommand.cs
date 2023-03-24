// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API;
using osu_bot.Bot.Callbacks;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using osu_bot.Modules.Converters;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Commands
{
    internal class LastCommand : ICommand
    {
        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;

        public string CommandText => "/last";

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text == null)
            {
                return;
            }

            if (message.From == null)
            {
                return;
            }

            UserScoreQueryParameters parameters = new(ScoreType.Recent, true);

            string args = message.Text.Trim()[CommandText.Length..];
            parameters.Parse(args);

            if (parameters.Username == null)
            {
                if (message.Text == CommandText)
                {
                    parameters.Limit = 1;
                }

                TelegramUser telegramUser = _database.TelegramUsers.FindOne(u => u.Id == message.From.Id);
                parameters.UserId = telegramUser != null
                    ? telegramUser.OsuUser.Id
                    : throw new Exception("Аккаунт Osu не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");
            }

            IList<OsuScore>? scores = await _service.GetUserScoresAsync(parameters);
            if (scores is null)
                throw new NotImplementedException();

            if (scores.Count == 0)
            {
                if (parameters.Mods == 0)
                {
                    throw new Exception($"У пользователя {parameters.Username} отсутствуют скоры за последние 24 часа");
                }
                else
                {
                    throw new Exception
                        ($"У пользователя {parameters.Username} отсутствуют скоры {ModsConverter.ToString(parameters.Mods)} за последние 24 часа");
                }
            }

            SKImage image;
            string? caption = null;
            InlineKeyboardMarkup? inlineKeyboard = null;
            if (scores.Count > 1)
            {
                image = await ImageGenerator.Instance.CreateScoresCardAsync(scores);
            }
            else
            {
                OsuScore score = scores.First();
                image = await ImageGenerator.Instance.CreateFullCardAsync(score);
                caption = score.Beatmap.Url;
                inlineKeyboard = Extensions.ScoreKeyboardMarkup(score.Beatmap.Id, score.Beatmap.BeatmapsetId);
            }
            Message answer = await botClient.SendPhotoAsync(
                chatId: message.Chat,
                caption: caption,
                photo: new InputOnlineFile(image.Encode().AsStream()),
                replyToMessageId: message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
