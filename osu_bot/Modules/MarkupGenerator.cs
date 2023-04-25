// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu_bot.API;
using osu_bot.Bot.Callbacks;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules.Converters;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Modules
{
    public class MarkupGenerator
    {
        public static MarkupGenerator Instance = new();

        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;

        private MarkupGenerator() { }

        #region Request callback markups
        private string GetRequireCallbackData(long requestId, string propertyName, object? newValue)
        {
            return $"{RequestCallback.DATA}:{requestId} A:{RequestCallbackAction.RequireChange} R:{propertyName} V:{newValue}";
        }

        private string GetSnipeRequireCallbackData(long requestId, string propertyName, object? newValue)
        {
            return $"{RequestCallback.DATA}:{requestId} A:{RequestCallbackAction.SRC} R:{propertyName} V:{newValue}";
        }

        private InlineKeyboardMarkup CreateUserSelectMarkup(Request request, string data, long chatId)
        {
            Match requestMatch = new Regex(@"P:(\d+)").Match(data);
            if (!requestMatch.Success)
                throw new Exception("При обработке запроса на реквест произошла ошибка");

            int page = int.Parse(requestMatch.Groups[1].Value);

            IEnumerable<TelegramUser> users = _database.TelegramUsers
                .Include(u => u.OsuUser)
                .Find(u => u.ChatId == chatId);
            int pagesCount = users.Count() / 8 + 1;
            users = users.Skip((page - 1) * 8).Take(8);
            IEnumerator<TelegramUser> usersEnumerator = users.GetEnumerator();
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Выбери своего бойца:"),
                InlineKeyboardButton.WithCallbackData("❌ Cancel", $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.Cancel}")
            });
            for (int i = 0; i < 4; i++)
            {
                List<InlineKeyboardButton> rowButtons = new();
                int j = 0;
                while (j < 2 && usersEnumerator.MoveNext())
                {
                    TelegramUser user = usersEnumerator.Current;
                    rowButtons.Add(InlineKeyboardButton.WithCallbackData(
                        text: user.OsuUser.Username,
                        callbackData: GetRequireCallbackData(request.Id, nameof(request.ToUser), user.Id)));
                    j++;
                }
                if (rowButtons.Any())
                    keyboard.Add(rowButtons);
            }

            List<InlineKeyboardButton> buttons = new();
            if (page != 1)
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Back", $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.PageChange} P:{page - 1}"));
            else
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Back"));

            buttons.Add(InlineKeyboardButton.WithCallbackData($"Page {page}/{pagesCount}"));

            if (page != pagesCount)
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next ▶️", $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.PageChange} P:{page + 1}"));
            else
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next ▶️"));

            keyboard.Add(buttons);

            return new InlineKeyboardMarkup(keyboard);
        }

        private async Task<InlineKeyboardMarkup> CreateSnipeSelectMarkup(Request request)
        {
            IList<OsuScore>? scores = await _service.GetUserBeatmapAllScoresAsync(request.Beatmap.Id, request.FromUser.OsuUser.Id);
            if (scores is null)
            {
                request.RequirePass = true;
                return CreateRequireEditMarkup(request);
            }
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Выбери свое позорище:"),
                InlineKeyboardButton.WithCallbackData("❌ Cancel", $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.SnipeCancel}")
            });
            foreach (OsuScore score in scores)
            {
                keyboard.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"{ModsConverter.ToString(score.Mods)} Score: 💎{score.Score.Separate(".")} 🎯{score.Accuracy:0.00}% 🏆{score.MaxCombo}/{score.BeatmapAttributes.MaxCombo}x",
                        callbackData: $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.Snipe} M:{score.Mods} S:{score.Score} C:{score.MaxCombo} F:{score.Accuracy:0.00}")
                });
            }
            return new InlineKeyboardMarkup(keyboard);
        }

        private InlineKeyboardMarkup CreateRequireEditMarkup(Request request)
        {
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
            List<InlineKeyboardButton> rowButtons1 = new()
            {
                InlineKeyboardButton.WithCallbackData(
                        text: request.RequirePass ? "🍀 Pass 🟢" : "🍀 Pass 🔴",
                        callbackData: GetRequireCallbackData(request.Id, nameof(request.RequirePass), !request.RequirePass)),
                InlineKeyboardButton.WithCallbackData(
                        text: request.RequireFullCombo ? "🔥 FC 🟢" : "🔥 FC 🔴",
                        callbackData: GetRequireCallbackData(request.Id, nameof(request.RequireFullCombo), !request.RequireFullCombo)),
            };
            if (request.ToUser.Id != request.FromUser.Id)
            {
                rowButtons1.Add(InlineKeyboardButton.WithCallbackData(
                        text: request.RequireSnipe ? "🎯 Snipe 🟢" : "🎯 Snipe 🔴",
                        callbackData: $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.SnipeSelect}"));
            }
            keyboard.Add(rowButtons1);

            List<InlineKeyboardButton> rowButtons2 = new();
            IEnumerable<Mod> mods = ModsConverter.ToMods(request.RequireMods);

            bool modHave = mods.Any(m => m.Name == NoMod.NAME);
            rowButtons2.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "NM ✅" : "NM ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - NoMod.NUMBER : request.RequireMods + NoMod.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModHardRock.NAME);
            rowButtons2.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "HR ✅" : "HR ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModHardRock.NUMBER : request.RequireMods + ModHardRock.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModDoubleTime.NAME);
            rowButtons2.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "DT ✅" : "DT ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModDoubleTime.NUMBER : request.RequireMods + ModDoubleTime.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModHidden.NAME);
            rowButtons2.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "HD ✅" : "HD ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModHidden.NUMBER : request.RequireMods + ModHidden.NUMBER))
            );

            keyboard.Add(rowButtons2);

            List<InlineKeyboardButton> rowButtons3 = new();

            modHave = mods.Any(m => m.Name == ModFlashlight.NAME);
            rowButtons3.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "FL ✅" : "FL ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModFlashlight.NUMBER : request.RequireMods + ModFlashlight.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModEasy.NAME);
            rowButtons3.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "EZ ✅" : "EZ ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModEasy.NUMBER : request.RequireMods + ModEasy.NUMBER))
            );

            modHave = mods.Any(m => m.Name == ModHalfTime.NAME);
            rowButtons3.Add(InlineKeyboardButton.WithCallbackData(
                text: modHave ? "HT ✅" : "HT ❌",
                callbackData: GetRequireCallbackData(
                    requestId: request.Id,
                    propertyName: nameof(request.RequireMods),
                    newValue: modHave ? request.RequireMods - ModHalfTime.NUMBER : request.RequireMods + ModHalfTime.NUMBER))
            );

            rowButtons3.Add(InlineKeyboardButton.WithCallbackData(
                text: request.IsOnlyMods ? "Only mods" : "Any mods",
                callbackData: GetRequireCallbackData(request.Id, nameof(request.IsOnlyMods), !request.IsOnlyMods))
            );

            keyboard.Add(rowButtons3);

            List<InlineKeyboardButton> rowButtons4 = new()
            {
                InlineKeyboardButton.WithCallbackData(
                    text: "✅ Send",
                    callbackData: $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.Save}"),

                InlineKeyboardButton.WithCallbackData(
                    text: "❌ Cancel",
                    callbackData: $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.Cancel}")
            };
            keyboard.Add(rowButtons4);

            return new InlineKeyboardMarkup(keyboard);
        }

        private InlineKeyboardMarkup CreateSnipeRequireEditMarkup(Request request)
        {
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Ну и что из этого надо снайпнуть?"),
            });

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: request.RequireSnipeScore ? $"💎 Snipe score: {request.Score.Separate(".")} 🟢" : $"💎 Snipe score: {request.Score.Separate(".")} 🔴",
                    callbackData: GetSnipeRequireCallbackData(request.Id, nameof(request.RequireSnipeScore), !request.RequireSnipeScore)),

            });

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: request.RequireSnipeAcc ? $"🎯 Snipe accuracy: {request.Accuracy}% 🟢" : $"🎯 Snipe accuracy: {request.Accuracy}% 🔴",
                    callbackData: GetSnipeRequireCallbackData(request.Id, nameof(request.RequireSnipeAcc), !request.RequireSnipeAcc)),

            });

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: request.RequireSnipeCombo ? $"🏆 Snipe combo: {request.Combo.Separate(".")}x 🟢" : $"🏆 Snipe combo: {request.Combo.Separate(".")}x 🔴",
                    callbackData: GetSnipeRequireCallbackData(request.Id, nameof(request.RequireSnipeCombo), !request.RequireSnipeCombo)),

            });

            keyboard.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: "✅ Send",
                    callbackData: $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.Save}"),

                InlineKeyboardButton.WithCallbackData(
                    text: "❌ Cancel",
                    callbackData: $"{RequestCallback.DATA}:{request.Id} A:{RequestCallbackAction.SnipeRequireCancel}")
            });

            return new InlineKeyboardMarkup(keyboard);
        }

        public async Task<InlineKeyboardMarkup> CreateRequestCallbackMarkup(RequestCallbackAction action, Request request, string callbackQueryData, long chatId)
        {
            return action switch
            {
                RequestCallbackAction.Create => CreateUserSelectMarkup(request, callbackQueryData, chatId),
                RequestCallbackAction.PageChange => CreateUserSelectMarkup(request, callbackQueryData, chatId),
                RequestCallbackAction.RequireChange => CreateRequireEditMarkup(request),
                RequestCallbackAction.SnipeSelect => await CreateSnipeSelectMarkup(request),
                RequestCallbackAction.SnipeCancel => CreateRequireEditMarkup(request),
                RequestCallbackAction.Snipe => CreateSnipeRequireEditMarkup(request),
                RequestCallbackAction.SRC => CreateSnipeRequireEditMarkup(request),
                RequestCallbackAction.SnipeRequireCancel => await CreateSnipeSelectMarkup(request),
                _ => ScoreKeyboardMarkup(request.Beatmap.Id, request.Beatmap.BeatmapsetId)
            };
        }

        public InlineKeyboardMarkup RequestKeyboardMakrup(Request request)
        {
            return new InlineKeyboardMarkup(
                new[]
                {
                   InlineKeyboardButton.WithUrl(text: "🌐 Beatmap URL", url: $"https://osu.ppy.sh/beatmaps/{request.Beatmap.Id}"),
                   InlineKeyboardButton.WithUrl(text: "⬇️ Beatmap mirror", url: $"https://beatconnect.io/b/{request.Beatmap.BeatmapsetId}"),
                });
        }
        #endregion

        public InlineKeyboardMarkup RequestsKeyboardMarkup(int[] requestsId, int page, int pagesCount, bool isDelete)
        {
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();

            List<InlineKeyboardButton> rowButtons1 = new()
            {
                InlineKeyboardButton.WithCallbackData("🚫 Hide", $"{RequestsListCallback.DATA} ID:{requestsId[1]} P:{page} Hide")
            };

            if (isDelete)
                rowButtons1.Add(InlineKeyboardButton.WithCallbackData("❌ Delete", $"{RequestsListCallback.DATA} ID:{requestsId[1]} P:{page} Delete"));

            keyboard.Add(rowButtons1);

            List<InlineKeyboardButton> rowButtons2 = new();

            if (page != 0)
                rowButtons2.Add(InlineKeyboardButton.WithCallbackData("◀️ Back", $"{RequestsListCallback.DATA} ID:{requestsId[0]} P:{page - 1}"));
            else
                rowButtons2.Add(InlineKeyboardButton.WithCallbackData("◀️ Back", $"{RequestsListCallback.DATA} ID:{requestsId[0]} P:{pagesCount - 1}"));

            rowButtons2.Add(InlineKeyboardButton.WithCallbackData($"Page {page + 1}/{pagesCount}"));

            if (page != pagesCount - 1)
                rowButtons2.Add(InlineKeyboardButton.WithCallbackData("Next ▶️", $"{RequestsListCallback.DATA} ID:{requestsId[2]} P:{page + 1}"));
            else
                rowButtons2.Add(InlineKeyboardButton.WithCallbackData("Next ▶️", $"{RequestsListCallback.DATA} ID:{requestsId[2]} P:0"));

            keyboard.Add(rowButtons2);   

            return new InlineKeyboardMarkup(keyboard);
        }

        public InlineKeyboardMarkup ScoreKeyoboardMarkup(OsuScore score)
        {
            if (score.Id is not -1 or 0)
                return ScoreKeyboardMarkup(score.Beatmap.Id, score.Beatmapset.Id, score.Id.ToString());
            else
                return ScoreKeyboardMarkup(score.Beatmap.Id, score.Beatmapset.Id);
        }

        public InlineKeyboardMarkup ScoreKeyboardMarkup(long beatmapId, long beatmapsetId, string? scoreId = null)
        {
            List<IEnumerable<InlineKeyboardButton>> keyboardButtons = new();
            keyboardButtons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "🎯 Мой скор", callbackData: $"{MyScoreCallback.DATA} beatmapId{beatmapId}"),
                InlineKeyboardButton.WithCallbackData(text: "🏆 Топ конфы", callbackData: $"{TopConferenceCallback.DATA} beatmapId{beatmapId}"),
                InlineKeyboardButton.WithCallbackData(text: "📌 Реквест", callbackData: $"{RequestCallback.DATA}:{beatmapId} A:{RequestCallbackAction.Create} P:1"),
            });

            if (scoreId is not null)
            {
                keyboardButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "🎬 Реплей", callbackData: $"{ReplayCallback.DATA} id:{scoreId} A:{ReplayCallbackAction.PressButton}")
                });
            }

            keyboardButtons.Add(new[]
                {
                    InlineKeyboardButton.WithUrl(text: "🌐 Beatmap URL", url: $"https://osu.ppy.sh/beatmaps/{beatmapId}"),
                    InlineKeyboardButton.WithUrl(text: "⬇️ Beatmap mirror", url: $"https://beatconnect.io/b/{beatmapsetId}"),
                });

            return new InlineKeyboardMarkup(keyboardButtons);
        }

        #region Replays markups
        public InlineKeyboardMarkup ReplayExistMarkup(string hash)
        {
            return new InlineKeyboardMarkup(new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "🔂 Существующий", callbackData: $"{ReplayCallback.DATA} id:{hash} A:{ReplayCallbackAction.SendExisting}"),
                    InlineKeyboardButton.WithCallbackData(text: "🆕 Новый", callbackData: $"{ReplayCallback.DATA} id:{hash} A:{ReplayCallbackAction.SendAgain}")
                },
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "❌ Cancel", callbackData : $"{ReplayCallback.DATA} id:{hash} A:{ReplayCallbackAction.Cancel}")
                }
            });
        }

        public InlineKeyboardMarkup ReplaySettingsChoose(long userId, int page, string hash)
        {
            IEnumerable<ReplaySettings> replaySettings = _database.ReplaySettings.Find(r => r.Owner.Id == userId);
            int pagesCount = replaySettings.Count() / 6 + 1;
            replaySettings = replaySettings.Skip((page - 1) * 6).Take(6);
            IEnumerator<ReplaySettings> enumerator = replaySettings.GetEnumerator();
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
            for (int i = 0; i < 4; i++)
            {
                List<InlineKeyboardButton> rowButtons = new();
                int j = 0;
                while (j < 2 && enumerator.MoveNext())
                {
                    ReplaySettings settings = enumerator.Current;
                    rowButtons.Add(InlineKeyboardButton.WithCallbackData(
                        text: settings.Name,
                        callbackData: $"{ReplayCallback.DATA}:{hash} A:{ReplayCallbackAction.Send} settings:{settings.Id}"));
                    j++;
                }
                if (rowButtons.Any())
                    keyboard.Add(rowButtons);
            }

            keyboard.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "❌ Cancel", callbackData: $"{ReplayCallback.DATA}:{hash} A:{ReplayCallbackAction.Cancel}")
            });

            List<InlineKeyboardButton> buttons = new();
            if (page != 1)
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Back", $"{ReplayCallback.DATA}:{hash} A:{ReplayCallbackAction.PageChange} P:{page - 1}"));
            else
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Back"));

            buttons.Add(InlineKeyboardButton.WithCallbackData($"Page {page}/{pagesCount}"));

            if (page != pagesCount)
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next ▶️", $"{ReplayCallback.DATA}:{hash} A:{ReplayCallbackAction.PageChange} P:{page + 1}"));
            else
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next ▶️"));

            keyboard.Add(buttons);

            return new InlineKeyboardMarkup(keyboard);
        }

        public InlineKeyboardMarkup ReplaySettingsList(long userId, int page)
        {
            IEnumerable<ReplaySettings> replaySettings = _database.ReplaySettings.Find(r => r.Owner.Id == userId);
            int pagesCount = replaySettings.Count() / 6 + 1;
            replaySettings = replaySettings.Skip((page - 1) * 6).Take(6);
            IEnumerator<ReplaySettings> enumerator = replaySettings.GetEnumerator();
            List<IEnumerable<InlineKeyboardButton>> keyboard = new();
            for (int i = 0; i < 4; i++)
            {
                List<InlineKeyboardButton> rowButtons = new();
                int j = 0;
                while (j < 2 && enumerator.MoveNext())
                {
                    ReplaySettings settings = enumerator.Current;
                    rowButtons.Add(InlineKeyboardButton.WithCallbackData(
                        text: settings.Name,
                        callbackData: $"{ReplaySettingsCallback.DATA}:{settings.Id} A:{ReplaySettingsCallbackAction.Select}"));
                    j++;
                }
                if (rowButtons.Any())
                    keyboard.Add(rowButtons);
            }

            WebAppInfo webApp = new()
            {
                Url = new ReplaySettings().GetWebPageString()
            };

            keyboard.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "🆕 Create", callbackData: $"{ReplaySettingsCallback.DATA}:0 A:{ReplaySettingsCallbackAction.Create}"),
                InlineKeyboardButton.WithCallbackData(text: "🚫 Hide", callbackData: $"{ReplaySettingsCallback.DATA}:0 A:{ReplaySettingsCallbackAction.Hide}")
            });

            List<InlineKeyboardButton> buttons = new();
            if (page != 1)
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Back", $"{ReplaySettingsCallback.DATA}:0 A:{ReplaySettingsCallbackAction.PageChange} P:{page - 1}"));
            else
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Back"));

            buttons.Add(InlineKeyboardButton.WithCallbackData($"Page {page}/{pagesCount}"));

            if (page != pagesCount)
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next ▶️", $"{ReplaySettingsCallback.DATA}:0 A:{ReplaySettingsCallbackAction.PageChange} P:{page + 1}"));
            else
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next ▶️"));

            keyboard.Add(buttons);

            return new InlineKeyboardMarkup(keyboard);
        }

        public InlineKeyboardMarkup ReplaySettingsSelect(int id)
        {
            ReplaySettings settings = _database.ReplaySettings.FindById(id);
            WebAppInfo webApp = new()
            {
                Url = settings.GetWebPageString()
            };

            return new InlineKeyboardMarkup(new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(settings.Name),
                },
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithWebApp("🖋 Edit", webApp),
                    InlineKeyboardButton.WithCallbackData("❌ Delete", $"{ReplaySettingsCallback.DATA}:{id} A:{ReplaySettingsCallbackAction.Delete}")
                },
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("◀️ Back", $"{ReplaySettingsCallback.DATA}:{id} A:{ReplaySettingsCallbackAction.Cancel}")
                }
            });
        }

        #endregion
    }
}
