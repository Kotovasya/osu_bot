using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using osu_bot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

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

            Parse(update.Message.Text);
            List<ScoreInfo> scores = await query.ExecuteAsync(API);
            if (scores.Count == 0)
            {
                if (query.Parameters.Mods == null)
                    throw new Exception($"У пользователя {query.Parameters.Username} отсутствуют скоры за последние 24 часа");
                else
                    throw new Exception
                        ($"У пользователя {query.Parameters.Username} отсутствуют скоры {ModsConverter.ToString(query.Parameters.Mods)} за последние 24 часа");
            }
            var image = scores.Count > 1 ? ImageGenerator.CreateScoresCard(scores) : ImageGenerator.CreateFullCard(scores.First());
            var imageStream = image.ToStream();
            await botClient.SendPhotoAsync(
                chatId: update.Message.Chat,
                photo: new InputOnlineFile(new MemoryStream(imageStream)),
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken);
        }

        //last<number> <username> <+MODS>
        private void Parse(string text)
        {
            var parameters = new UserLastScoreQueryParameters();
            query.Parameters = parameters;
            if (text == Text)
            {
                parameters.Username = "Kotovasya";
                return;
            }

            string[] args = text[text.IndexOf(' ')..]
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length > 4)
                throw new ArgumentException("В запросе более 4-х аргументов. Синтаксис: /last <number> <username> <+MODS> <+pass>");

            foreach (var arg in args)
            {
                if (int.TryParse(arg, out int number))
                    parameters.Limit = number;
                
                else if (arg == "+pass")
                    parameters.IncludeFails = false;
               
                else if (arg.StartsWith('+'))
                {
                    var parameterMods = new HashSet<Mod>();
                    parameters.Mods = parameterMods;

                    string modsString = arg.Remove(0, 1);
                    if (modsString.Length < 2 || modsString.Length % 2 != 0)
                        throw new ModsArgumentException();

                    var modsStrings = modsString.Split(2);
                    foreach (var modString in modsStrings)
                        parameterMods.Add(ModsConverter.ToMod(modString));
                }
                else
                    parameters.Username = arg ?? "Kotovasya";
            }

            parameters.Username ??= "Kotovasya";
        }
    }
}
