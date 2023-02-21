using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using osu_bot.Modules;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using static System.Net.Mime.MediaTypeNames;

namespace osu_bot.Bot.Commands.Main
{
    //top <number> <username> <+MODS>
    public class TopCommand : Command
    {
        private readonly UserScoresQuery query = new();

        public override string Text => "/top";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null)
                return;

            if (update.Message.Text == null)
                return;

            Parse(update.Message);

            List<ScoreInfo> scores = await query.ExecuteAsync(API);
            if (scores.Count == 0)
            {
                if (query.Parameters.Mods == null)
                    throw new Exception($"У пользователя {query.Parameters.Username} отсутствуют топ скоры");
                else
                    throw new Exception
                        ($"У пользователя {query.Parameters.Username} отсутствуют топ скоры с {ModsConverter.ToString(query.Parameters.Mods)}");
            }
            var image = scores.Count > 1 ? ImageGenerator.CreateScoresCard(scores) : ImageGenerator.CreateFullCard(scores.First());
            var imageStream = image.ToStream();
            await botClient.SendPhotoAsync(
                chatId: update.Message.Chat,
                photo: new InputOnlineFile(new MemoryStream(imageStream)),
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken);
        }

        private void Parse(Message message)
        {
            var parameters = new UserTopScoreQueryParameters();
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
