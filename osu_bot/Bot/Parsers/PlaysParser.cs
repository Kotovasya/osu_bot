// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using osu_bot.Modules.Converters;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.Bot.Parsers
{
    public class PlaysParser : Parser
    {
        private readonly BotHandle _botHandle;

        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;
        private readonly UserScoreQueryParameters _parameters = new(ScoreType.Recent, false)
        {
            LimitApi = 20,
            Limit = 20,
        };

        private readonly OsuBeatmapStatus[] _includeStatuses = new[] { OsuBeatmapStatus.Wip, OsuBeatmapStatus.Graveyard, OsuBeatmapStatus.Pending };

        protected override TimeSpan Delay => TimeSpan.FromMinutes(30);

        public PlaysParser(BotHandle botHandle)
        {
            _botHandle = botHandle;
        }

        protected override async Task ActionAsync()
        {
            List<TelegramUser> users = _database.TelegramUsers
                .FindAll()
                .ToList();

            foreach(TelegramUser user in users)
            {
                _parameters.UserId = user.OsuUser.Id;
                IList<OsuScore>? lastScores = await _service.GetUserScoresAsync(_parameters);
                if (lastScores is null)
                    return;

                await CheckRequestsComplete(user, lastScores);

                foreach (OsuScore lastScore in lastScores)
                {
                    if (_database.Scores.FindById(lastScore.Id) != null)
                        continue;

                    if (_includeStatuses.Any(s => s != lastScore.Beatmap.Status))
                        continue;

                    OsuScore? findScore = _database.Scores
                        .Find(s => s.Beatmap.Id == lastScore.Beatmap.Id)
                        .Where(s => s.User.Id == user.Id)                       
                        .FirstOrDefault(s => s.Mods == lastScore.Mods);

                    if (findScore is null)
                    {
                        _database.Beatmaps.Upsert(lastScore.Beatmap);
                        _database.Beatmapsets.Upsert(lastScore.Beatmapset);
                        _database.BeatmapAttributes.Upsert(lastScore.BeatmapAttributes);
                        _database.Scores.Insert(lastScore);
                    }
                    else if (lastScore.Score > findScore.Score)
                    {
                        _database.Scores.Delete(findScore.Id);
                        _database.Scores.Insert(lastScore);
                    }
                }
            }
        }

        private async Task CheckRequestsComplete(TelegramUser user, IList<OsuScore> scores)
        {
            IList<Request> requests = _database.Requests
                .Find(r => r.ToUser.Id == user.Id)
                .Where(r => !r.IsComplete && !r.IsTemporary)
                .ToList();
            foreach(Request request in requests)
            {
                OsuScore? score = scores.FirstOrDefault(s => s.Beatmap.Id == request.Beatmap.Id);
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

                SKImage image = await ImageGenerator.Instance.CreateFullCardAsync(score);

                string textMessage;
                if (!request.RequireSnipe)
                {

                }
                else
                    textMessage = $"{request.ToUser.OsuUser.Username} "

                ChatMember fromMember = await _botHandle.BotClient.GetChatMemberAsync(
                    chatId: _botHandle.ChatId,
                    userId: request.FromUser.Id);

                ChatMember toMember = await _botHandle.BotClient.GetChatMemberAsync(
                    chatId: _botHandle.ChatId,
                    userId: request.ToUser.Id);

                await _botHandle.BotClient.SendPhotoAsync(
                    chatId: _botHandle.ChatId,
                    
                    )
            }
        }
    }
}
