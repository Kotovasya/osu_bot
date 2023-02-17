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
            if (update.Message?.Text != null)
                return;

            Parse(update.Message.Text);
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

        private void Parse(string text)
        {
            var parameters = new UserTopScoreQueryParameters();
            query.Parameters = parameters;
            if (text == Text)
            {
                if (Database.TelegramUsers.Find)
                return;
            }

            string[] args = text[text.IndexOf(' ')..]
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length > 2)
                throw new ArgumentException("В запросе более 2-х аргументов. Синтаксис: /top <number> <username> <+MODS>");     

            foreach (var arg in args)
            {
                if (int.TryParse(arg, out int number))
                {
                    parameters.Offset = number - 1;
                    parameters.Limit = 1;
                }
                else if (arg.StartsWith('+'))
                {
                    var parameterMods = new HashSet<Mod>();
                    parameters.Mods = parameterMods;

                    string modsString = arg.Remove(0, 1);
                    if (modsString.Length < 2 || modsString.Length % 2 != 0)
                        throw new ModsArgumentException();

                    var mods = modsString.Split(2);
                    foreach(var mod in mods)
                        parameterMods.Add(ModsConverter.ToMod(mod));
                }
                else
                    parameters.Username = arg ?? "Kotovasya";
            }

            parameters.Username ??= "Kotovasya";
        }
    }
}
