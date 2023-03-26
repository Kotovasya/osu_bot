// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using osu_bot.API;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules.Converters;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.Bot.Parsers
{
    public class PlaysParser : Parser
    {
        private readonly DatabaseContext _database = DatabaseContext.Instance;
        private readonly OsuService _service = OsuService.Instance;
        private readonly UserScoreQueryParameters _parameters = new(ScoreType.Recent, false)
        {
            Limit = 100,
            MapsStatusOnly = new[] { OsuBeatmapStatus.Graveyard, OsuBeatmapStatus.Pending, OsuBeatmapStatus.Wip }
        };

        protected override TimeSpan Delay => TimeSpan.FromMinutes(30);

        protected override async Task ActionAsync()
        {
            List<TelegramUser> users = _database.TelegramUsers
                .FindAll()
                .ToList();

            foreach(TelegramUser user in users)
            {
                _parameters.UserId = user.OsuUser.Id;
                IEnumerable<OsuScore>? lastScores = await _service.GetUserScoresAsync(_parameters);
                if (lastScores is null)
                    throw new NotImplementedException();

                foreach (OsuScore lastScore in lastScores)
                {
                    var testScore = _database.Scores.FindById(lastScore.Id);
                    if (_database.Scores.FindById(lastScore.Id) != null)
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
    }
}
