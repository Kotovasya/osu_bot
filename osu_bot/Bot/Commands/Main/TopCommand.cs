using Newtonsoft.Json.Linq;
using osu_bot.API.Queries;
using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace osu_bot.Bot.Commands.Main
{
    //top <number> <username> <+MODS>
    public class TopCommand : Command
    {
        private readonly TopScoreQuery query = new();

        public override string Text => "/top";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text != null)
            {
                var messageText = update.Message.Text;
                string[] args = messageText
                    .Substring(messageText.IndexOf(' '), messageText.Length - messageText.IndexOf(' '))
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (args.Length > 2)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: "В запросе более 2-х аргументов. Синтаксис: /top <number> <username> <+MODS>",
                        replyToMessageId: update.Message.MessageId,
                        cancellationToken: cancellationToken);
                    return;
                }

                if (args.Length == 0)
                {
                    query.UserId = 15833700;
                }

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
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: "Неправильно указаны моды, пример: +DTHD, +NM, +HRNF",
                                replyToMessageId: update.Message.MessageId,
                                cancellationToken: cancellationToken);
                            return;
                        }
                        query.Mods = ModsParser.ConvertToMods(arg);
                    }
                    else
                    {
                        var id = await API.GetIdByUsernameAsync(arg);
                        query.UserId = id;
                    }
                }

                List<BeatmapScore> scores = await query.ExecuteAsync(API);
            }
        }
    }
}
