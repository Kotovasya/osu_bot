// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Bot;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using SkiaSharp;
using static System.Formats.Asn1.AsnWriter;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace osu_bot.API.Checkers
{
    public class RequestsChecker : IChecker<IList<OsuScore>>
    {
        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly BotHandle _botHandle;

        public RequestsChecker(BotHandle botHandle)
        {
            _botHandle = botHandle;
        }

        public async Task CheckAsync(IList<OsuScore> value)
        {
            if (value.Count == 0)
                return;

            OsuScore firstScore = value.First();
            TelegramUser user = _database.TelegramUsers.FindOne(u => u.OsuUser.Id == firstScore.User.Id);

            if (user is null)
                return;

            IList<Request> requests = _database.Requests
                .Include(r => r.FromUser)
                .Include(r => r.FromUser.OsuUser)
                .Include(r => r.ToUser)
                .Include(r => r.ToUser.OsuUser)
                .Find(r => r.ToUser.Id == user.Id)
                .Where(r => !r.IsComplete && !r.IsTemporary)
                .ToList();

            foreach (Request request in requests)
            {
                OsuScore? score = value.FirstOrDefault(s => s.Beatmap.Id == request.Beatmap.Id);
                if (score is null)
                    continue;

                bool isRequestMods = false;
                if (request.IsOnlyMods)
                {
                    int requestModsWithoutHidden = request.RequireMods - ModHidden.NUMBER;
                    int requestModsWithHidden = request.RequireMods + ModHidden.NUMBER;
                    if (request.RequireMods == score.Mods)
                        isRequestMods = true;
                    else if ((requestModsWithoutHidden == ModHardRock.NUMBER || requestModsWithoutHidden == ModDoubleTime.NUMBER)
                            && score.Mods == requestModsWithoutHidden)
                    {
                        isRequestMods = true;
                    }
                    else if ((requestModsWithHidden == ModHardRock.NUMBER + ModHidden.NUMBER || requestModsWithHidden == ModDoubleTime.NUMBER + ModHidden.NUMBER)
                        && score.Mods == requestModsWithHidden)
                    {
                        isRequestMods = true;
                    }
                }
                if (!isRequestMods)
                    continue;

                bool isRequestComplete = false;
                if (request.RequirePass && score.IsPassed)
                    isRequestComplete = true;
                else if (request.RequireFullCombo && score.IsFullCombo)
                    isRequestComplete = true;
                else if (request.RequireSnipeScore && score.Score > request.Score)
                    isRequestComplete = true;
                else if (request.RequireSnipeAcc && score.Accuracy > request.Accuracy)
                    isRequestComplete = true;
                else if (request.RequireSnipeCombo && score.MaxCombo > request.Combo)
                    isRequestComplete = true;

                if (!isRequestComplete)
                    continue;

                request.IsComplete = true;
                request.DateComplete = DateTime.Now;

                _database.Requests.Upsert(request);

                ChatMember fromMember = await _botHandle.BotClient.GetChatMemberAsync(
                    chatId: _botHandle.ChatId,
                    userId: request.FromUser.Id);

                ChatMember toMember = await _botHandle.BotClient.GetChatMemberAsync(
                    chatId: _botHandle.ChatId,
                    userId: request.ToUser.Id);

                int fromRequestsCompleteCount = _database.Requests
                    .Find(r => r.IsComplete)
                    .Where(r => r.FromUser.Id == request.ToUser.Id && r.ToUser.Id == request.FromUser.Id)
                    .Count();

                int toRequestsCompleteCount = _database.Requests
                    .Find(r => r.IsComplete)
                    .Where(r => r.FromUser.Id == request.FromUser.Id && r.ToUser.Id == request.ToUser.Id)
                    .Count();

                string textMessage;
                if (request.RequirePass)
                    textMessage = $"{request.ToUser.OsuUser.Username} выполнил реквест на пасс от @{fromMember.User.Username}";
                else if (request.RequireFullCombo)
                    textMessage = $"{request.ToUser.OsuUser.Username} выполнил реквест на фк от @{fromMember.User.Username}";
                else
                {
                    textMessage = $"{request.ToUser.OsuUser.Username} выполнил реквест снайпа от @{fromMember.User.Username} выбив";
                    if (request.RequireSnipeScore)
                        textMessage += $"скор выше заданного {request.Score.Separate(".")}";
                    else if (request.RequireSnipeAcc)
                        textMessage += $"аккураси выше заданной {request.Accuracy}%";
                    else
                        textMessage += $"комбо выше заданного {request.Combo}x";

                    textMessage += $"\nСчёт реквестов:\n{request.ToUser.OsuUser.Username} {toRequestsCompleteCount} - {fromRequestsCompleteCount} {request.FromUser.OsuUser.Username}";
                }

                using SKImage image = await ImageGenerator.Instance.CreateFullCardAsync(score);

                await _botHandle.BotClient.SendPhotoAsync(
                    chatId: _botHandle.ChatId,
                    caption: textMessage,
                    photo: new InputOnlineFile(image.Encode().AsStream()));

                Task.Delay(300).Wait();
            }
        }
    }
}
