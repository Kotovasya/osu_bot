// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API;
using osu_bot.Bot.Callbacks;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Exceptions;
using osu_bot.Modules;
using osu_bot.Modules.Converters;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Commands
{
    //top <number> <username> <+MODS>
    public class TopCommand : ICommand
    {
        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;

        public string CommandText => "/top";

        public async Task ActionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text == null)
                return;

            if (message.From == null)
                return;
            

            UserScoreQueryParameters parameters = new(ScoreType.Best);

            string args = message.Text.Trim()[CommandText.Length..];
            parameters.Parse(args);

            if (parameters.Username == null || message.Text == CommandText)
            {
                TelegramUser telegramUser = _database.TelegramUsers
                    .Include(u => u.OsuUser)
                    .FindOne(u => u.Id == message.From.Id);

                if (telegramUser is not null)
                    parameters.Username = telegramUser.OsuUser.Username;
                else
                    throw new UserNotRegisteredException();
            }

            IList<OsuScore>? scores = await _service.GetUserScoresAsync(parameters);

            if (scores is null || scores.Count == 0)
                throw new UserScoresNotFound(parameters.Username, ScoreType.Best, parameters.Mods);

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

            await botClient.SendPhotoAsync(
                chatId: message.Chat,
                caption: caption,
                photo: new InputOnlineFile(image.Encode().AsStream()),
                replyToMessageId: message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
