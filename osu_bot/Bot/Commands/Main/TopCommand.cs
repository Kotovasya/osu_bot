using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Exceptions;
using osu_bot.Images;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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
        private readonly ScoresQuery query = new(new TopScoreQueryParameters());

        public override string Text => "/top";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text != null)
            {
                await Parse(update.Message.Text);
                List<ScoreInfo> scores = await query.ExecuteAsync(API);
                var imageStream = ImageGenerator.CreateScoresCard(scores).ToStream();
                await botClient.SendPhotoAsync(
                    chatId: update.Message.Chat,
                    photo: new InputOnlineFile(imageStream),
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken);
            }
        }

        private async Task Parse(string text)
        {
            if (text == Text)
            {
                query.Parameters.UserId = 15833700;
                return;
            }

            string[] args = text[text.IndexOf(' ')..]
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length > 2)
                throw new ArgumentException("В запросе более 2-х аргументов. Синтаксис: /top <number> <username> <+MODS>");
            
            if (args.Length == 0)
                query.Parameters.UserId = 15833700;

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
                {
                    var id = await API.GetIdByUsernameAsync(arg);
                    query.Parameters.UserId = id;
                }
            }
        }
    }
}
