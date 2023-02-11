using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
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
        private readonly UserScoresQuery query = new(new UserTopScoreQueryParameters());

        public override string Text => "/top";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text != null)
            {
                await Parse(update.Message.Text);
                List<ScoreInfo> scores = await query.ExecuteAsync(API);
                if (scores.Count == 0)
                {
                    if (query.Parameters.Mods == Mods.ALL)
                        throw new Exception($"У пользователя {query.Parameters.Username} отсутствуют топ скоры");
                    else
                        throw new Exception
                            ($"У пользователя {query.Parameters.Username} отсутствуют топ скоры с модами {ModsParser.ConvertToString(query.Parameters.Mods)}");
                }
                var image = ImageGenerator.CreateScoresCard(scores);
                var imageStream = image.ToStream();
                await botClient.SendPhotoAsync(
                    chatId: update.Message.Chat,
                    photo: new InputOnlineFile(new MemoryStream(imageStream)),
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);
            }
        }

        private async Task Parse(string text)
        {
            if (text == Text)
            {
                query.Parameters.Username = "Kotovasya";
                return;
            }

            string[] args = text[text.IndexOf(' ')..]
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length > 2)
                throw new ArgumentException("В запросе более 2-х аргументов. Синтаксис: /top <number> <username> <+MODS>");
            
            if (args.Length == 0)
                query.Parameters.Username = "Kotovasya";

            foreach (var arg in args)
            {
                if (int.TryParse(arg, out int number))
                {
                    query.Parameters.Offset = number - 1;
                    query.Parameters.Limit = 1;
                }
                else if (arg.StartsWith('+'))
                {
                    var mods = arg.Remove(0, 1);
                    if (mods.Length < 2 || mods.Length % 2 != 0)
                        throw new ModsArgumentException();

                    query.Parameters.Mods = ModsParser.ConvertToMods(arg);
                }
                else
                    query.Parameters.Username = arg ?? "Kotovasya";
            }
        }
    }
}
