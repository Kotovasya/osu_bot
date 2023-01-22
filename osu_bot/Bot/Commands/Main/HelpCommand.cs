using osu_bot.Bot.Callbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Commands.Main
{
    public class HelpCommand : Command
    {
        public override string Text => "/help";

        public override async Task ActionAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "⚙️Основные", callbackData: HelpCallback.DATA),
                    InlineKeyboardButton.WithCallbackData(text: "🗺Карты", callbackData: MapsCallback.DATA)
                });

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat,
                text: "Информация о каких командах тебя интересует?",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}
