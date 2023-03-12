// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using osu_bot.API.Parameters;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules;

namespace osu_bot.Bot.Parsers
{
    public class PlaysParser : Parser
    {
        private readonly UserScoresQuery _userScoresQuery = new();

        private readonly DatabaseContext _database = DatabaseContext.Instance;

        protected override TimeSpan Delay => TimeSpan.FromMinutes(30);

        private void SetQueryParameters(string username)
        {
            _userScoresQuery.Parameters.Username = username;
            _userScoresQuery.Parameters.IncludeFails = false;
            _userScoresQuery.Parameters.Limit = 100;
            _userScoresQuery.Parameters.IsRecent = true;
        }

        protected override async Task ActionAsync()
        {
            List<TelegramUser> users = DatabaseContext.Instance.TelegramUsers.FindAll().ToList();
            foreach(TelegramUser user in users)
            {
                SetQueryParameters(user.OsuName);
                IEnumerable<OsuScoreInfo> lastScores = await _userScoresQuery.ExecuteAsync();
                lastScores = lastScores.Where(s => !s.Beatmap.ScoresTable);

                foreach (OsuScoreInfo lastScore in lastScores)
                {
                    if (_database.Scores.FindById(lastScore.Id) != null)
                        continue;

                    ScoreInfo? findScore = _database.Scores
                        .Find(s => s.BeatmapId == lastScore.Beatmap.Id)
                        .Where(s => s.User.Id == user.Id)                       
                        .FirstOrDefault(s => s.Mods == ModsConverter.ToInt(lastScore.Mods));

                    if (findScore != null)
                    {
                        if (lastScore.Score > findScore.Score)
                            _database.Scores.Delete(findScore.Id);
                        else
                            continue;
                    }

                    ScoreInfo score = new(lastScore)
                    {
                        User = user,
                    };

                    _database.Scores.Insert(score);
                }
            }
        }
    }
}
