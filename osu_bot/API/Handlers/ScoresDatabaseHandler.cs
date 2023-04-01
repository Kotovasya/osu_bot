// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using Telegram.Bot.Types;

namespace osu_bot.API.Handlers
{
    public class ScoresDatabaseHandler : IHandler<IList<OsuScore>>
    {
        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task HandlingAsync(IList<OsuScore> value)
        {
            if (value.Count == 0)
                return;

            OsuScore firstScore = value.First();
            TelegramUser user = _database.TelegramUsers.FindOne(u => u.OsuUser.Id == firstScore.User.Id);

            if (user is null)
                return;


            foreach (OsuScore score in value)
            {
                if (_database.Scores.FindById(score.Id) != null)
                    continue;

                if (!score.IsPassed)
                    continue;

                if (score.Beatmap.Status is OsuBeatmapStatus.Ranked or OsuBeatmapStatus.Loved or OsuBeatmapStatus.Qualified)
                    continue;

                OsuScore? findScore = _database.Scores
                        .Find(s => s.Beatmap.Id == score.Beatmap.Id)
                        .Where(s => s.User.Id == user.Id)
                        .FirstOrDefault(s => s.Mods == score.Mods);

                if (findScore is null)
                {
                    _database.Beatmaps.Upsert(score.Beatmap);
                    _database.Beatmapsets.Upsert(score.Beatmapset);
                    _database.BeatmapAttributes.Upsert(score.BeatmapAttributes);
                    _database.Scores.Insert(score);
                }
                else if (score.Score > findScore.Score)
                {
                    _database.Scores.Delete(findScore.Id);
                    _database.Scores.Insert(score);
                }
            }
        }
    }
}
