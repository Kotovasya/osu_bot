// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules;

namespace osu_bot.Bot.Parsers
{
    public class PlaysParser
    {
        private readonly TimeSpan _delay = TimeSpan.FromMinutes(30);

        private readonly UserScoresQuery _query = new();

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public PlaysParser()
        {
            _query.Parameters.IncludeFails = false;
            _query.Parameters.Limit = 100;
            _query.Parameters.IsRecent = true;
        }

        private async void Action()
        {
            List<TelegramUser> users = DatabaseContext.Instance.TelegramUsers.FindAll().ToList();
            foreach(TelegramUser user in users)
            {
                List<OsuScoreInfo> lastScores = await _query.ExecuteAsync();
                foreach (OsuScoreInfo lastScore in lastScores)
                {
                    if (_database.Scores.FindById(lastScore.Id) != null)
                        continue;

                    IEnumerable<ScoreInfo> findScores = _database.Scores
                        .Find(s => s.BeatmapId == lastScore.Id)
                        .Where(s => s.Mods == ModsConverter.ToInt(lastScore.Mods));

                    if (findScores.All(s => s.Score > lastScore.Score && s.PP >= lastScore.PP))
                        continue;                   

                    ScoreInfo score = new(lastScore)
                    {
                        User = user
                    };
                    _database.Scores.Insert(score);
                }
            }
        }

        public async void Run()
        {
            while (true)
            {
                Action();
                Task.Delay(_delay).Wait();
            }
        }
    }
}
