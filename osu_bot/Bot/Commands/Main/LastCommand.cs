using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Bot.Callbacks;
using osu_bot.Entites;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using osu_bot.Modules;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Commands.Main
{
    internal class LastCommand : Command
    {
        private readonly UserScoresQuery query = new();

        public override string Text => "/last";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text == null)
                return;

            Parse(update.Message);
            List<ScoreInfo> scores = await query.ExecuteAsync(API);
            if (scores.Count == 0)
            {
                if (query.Parameters.Mods == null)
                    throw new Exception($"У пользователя {query.Parameters.Username} отсутствуют скоры за последние 24 часа");
                else
                    throw new Exception
                        ($"У пользователя {query.Parameters.Username} отсутствуют скоры {ModsConverter.ToString(query.Parameters.Mods)} за последние 24 часа");
            }

            byte[]? imageStream;
            string? caption = null;
            InlineKeyboardMarkup? inlineKeyboard = null;
            if (scores.Count > 1)
                imageStream = ImageGenerator.CreateScoresCard(scores).ToStream();
            else
            {
                var score = scores.First();
                imageStream = ImageGenerator.CreateFullCard(score).ToStream();
                caption = score.Beatmap.Url;
                inlineKeyboard = new(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "🎯Мой скор", callbackData: $"{MyScoreCallback.DATA} beatmapId{score.Beatmap.Id})"),
                        InlineKeyboardButton.WithCallbackData(text: "🏆Топ конфы", callbackData: MapsCallback.DATA)
                    });
            }

            await botClient.SendPhotoAsync(
                chatId: update.Message.Chat,
                caption: caption,
                photo: new InputOnlineFile(new MemoryStream(imageStream)),
                replyToMessageId: update.Message.MessageId,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        //last<number> <username> <+MODS>
        private void Parse(Message message)
        {
            var parameters = new UserLastScoreQueryParameters();
            query.Parameters = parameters;

            var text = message.Text.Trim();

            text = text[Text.Length..];
            int endIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                {
                    int startIndex = i;
                    while (text.Length > i && char.IsDigit(text[i]))
                        i++;

                    string result = text[startIndex..i];
                    parameters.Offset = 0;
                    parameters.Limit = int.Parse(result);
                    endIndex = i;
                }

                else if (text[i] == '+')
                {
                    int startIndex = ++i;
                    while (text.Length > i && char.IsLetterOrDigit(text[i]))
                        i++;

                    string result = text[startIndex..i];
                    endIndex = i;

                    if (int.TryParse(result, out int number))
                    {
                        parameters.Offset = number - 1; ;
                        parameters.Limit = 1;
                    }
                    else if (result == "pass")
                        parameters.IncludeFails = false;
                    else
                    {
                        var parameterMods = new HashSet<Mod>();
                        parameters.Mods = parameterMods;

                        if (result.Length < 2 || result.Length % 2 != 0)
                            throw new ModsArgumentException();

                        var modsStrings = result.Split(2);
                        foreach (var modString in modsStrings)
                            parameterMods.Add(ModsConverter.ToMod(modString));
                    }
                }
            }

            if (text.Length > endIndex)
                parameters.Username = text.Substring(endIndex + 1, text.Length - endIndex - 1);

            if (parameters.Username == null || message.Text == Text)
            {
                var telegramUser = Database.TelegramUsers.FindOne(u => u.Id == message.From.Id);
                if (telegramUser != null)
                    parameters.Username = telegramUser.OsuName;
                else
                    throw new Exception("Аккаунт Osu не привязан к твоему телеграм аккаунту. Используй /reg [username] для привязки");
            }
        }
    }
}
