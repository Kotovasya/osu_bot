// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Bot.Callbacks
{
    public class HelpCallback : ICallback
    {
        private readonly string[] _descriptions =
        {
@"ГЛАВНОЕ

1. Команды необходимо писать только ОТДЕЛЬНЫМ сообщением
2. НЕ ставить лишние знаки, пробелы между аргументами команд
3. В любой команде при запросе по нику, ник указывается в КОНЦЕ

В описаниях команд имеются следующие обозначения:
<> - необязательный аргумент
[] - обязательный аргумент
В случае, если в команде возможно указание имя пользователя, но оно не указывается, используется имя привязанного аккаунта Osu! командой /reg
В случае, если в команде возможно указание модов, но они не указываются, поиск будет осуществлен по всем модам
Моды указываются полностью, при поиске +DT скоры с NFDT или DTHD показываться не будут

Список работающих модов:
NM - No Mod
EZ - Easy
HT - Half Time
NF - No Fail
HR - Hard Rock
DT - Double Time
HD - Hidden
FL - Flashlight
SO - Spin Out",

@"Команда /reg
Привязывает аккаунт osu! к аккаунту Telegram

Синтаксис: /reg [username]

Варианты использования:
/reg Kotovasya - привязать аккаунт с именем Kotovasya",

@"Команда /stats
Показывает свою или указанного игрока статистику

Синтаксис: /stats <username>

Варианты использования:
/stats - показать статистику привязанного аккаунта
/stats Soorek - показать статистику игрока Soorek",

@"Команда /top
Показать свой или указанного игрока топ скор(ы)

Синтаксис: /top <<+>N> <+MODS> <username>
1. Без указания аргумента N, отображает 5 скоров
2. При указании аргумента N, отображает N скоров
3. При указании аргумента +N, отображает подробный N-й по счету скор
4. При использовании аргументов +N и +MODS, аргумент +N не учитывается

Варианты использования:
/top - показать 5 топ скоров привязанного аккаунта
/top +DTHD - показать 5 топ скоров с модами DTHD привязанного аккаунта 
/top 10 Soorek - показать 10 топ скоров игрока Soorek
/top +69 Soorek - показать подробный 69-й топ скор игрока Soorek
/top 7 +NM Soorek - показать 7 топ скоров без модов игрока Soorek",

@"Команда /last
Показать свой или указанного игрока последние скоры за 24 часа

Синтаксис /last <<+>N> <+MODS> <+pass> <username>
1. Без указания аргумента N, отображает подробный последний скор
2. При указании аргумента N, отображает последнее N-ее кол-во скоров
3. При указании аргумента +N, отображает подробный последний N-й по счету скор
4. При использовании аргументов +N и +MODS, аргумент +N не учитывается
5. При использовании аргумента +pass показывает только сабмитнутые скоры

Варианты использования:
/last - показать последний скор привязанного аккаунта
/last +DTHD - показать последний скор с модами DTHD привязанного аккаунта 
/last 10 Soorek - показать последние 10 скоров игрока Soorek
/last +69 Soorek - показать последний 69-й последний скор игрока Soorek
/last 7 +NM +pass Soorek - показать 7 последних пасснутых скоров без модов игрока Soorek",
        };

        private int _currentPage = 0;

        public const string DATA = "Help callback";

        public string Data => DATA;

        public async Task<CallbackResult?> ActionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
                return null;

            if (callbackQuery.Message == null)
                return null;
            

            string data = callbackQuery.Data;
            Match pageMatch = new Regex(@"p(\d+)").Match(data);

            if (pageMatch.Success)
            {
                _currentPage = int.Parse(pageMatch.Groups[1].Value);
            }

            List<InlineKeyboardButton> buttons = new();

            if (_currentPage != 0)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData(text: "◀️Назад", callbackData: $"{Data} p{_currentPage - 1}"));
            }
            else
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️Назад"));
            }

            buttons.Add(InlineKeyboardButton.WithCallbackData($"Page {_currentPage + 1}/{_descriptions.Length}"));

            if (_currentPage != _descriptions.Length - 1)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData(text: "Вперед▶️", callbackData: $"{Data} p{_currentPage + 1}"));
            }
            else
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData("Вперед▶️"));
            }

            InlineKeyboardMarkup inlineKeyboard = new(buttons);

            if (data == DATA)
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat,
                    text: _descriptions[_currentPage],
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);
            else
                await botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat,
                    messageId: callbackQuery.Message.MessageId,
                    text: _descriptions[_currentPage],
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);

            return CallbackResult.Empty();
        }
    }
}
