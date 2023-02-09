using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace osu_bot.Bot.Commands.Main
{
    //top <number> <username> <+MODS>
    public class TopCommand : Command
    {
        private readonly TopScoreQuery query = new()
        {
            IncludeFails = false,
        };

        public override string Text => "/top";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text != null)
            {
                try
                {
                    await Parse(update.Message.Text);
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: ex.Message,
                        replyToMessageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                }

                List<ScoreInfo> scores = await query.ExecuteAsync(API);
            }
        }

        private async Task Parse(string text)
        {
            string[] args = text
                    .Substring(text.IndexOf(' '), text.Length - text.IndexOf(' '))
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (args.Length > 2)
                throw new ArgumentException("В запросе более 2-х аргументов. Синтаксис: /top <number> <username> <+MODS>");
            
            if (args.Length == 0)
                query.UserId = 15833700;

            foreach (var arg in args)
            {
                if (int.TryParse(arg, out int number))
                {
                    query.Offset = number - 1;
                    query.Limit = 1;
                }
                else if (arg.StartsWith('+'))
                {
                    var mods = arg.Remove(0, 1);
                    if (mods.Length < 2 || mods.Length % 2 != 0)
                        throw new ModsArgumentException();

                    query.Mods = ModsParser.ConvertToMods(arg);
                }
                else
                {
                    var id = await API.GetIdByUsernameAsync(arg);
                    query.UserId = id;
                }
            }
        }
    }
}
