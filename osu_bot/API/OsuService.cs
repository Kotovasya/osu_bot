// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Modules.Converters;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace osu_bot.API
{
    public class OsuService
    {
        public static OsuService Instance { get; } = new();

        private OsuService() { }

        private readonly OsuAPI _api = OsuAPI.Instance;
        private readonly DatabaseContext _database = DatabaseContext.Instance;

        public async Task<OsuUser?> GetUserAsync(long id)
        {
            return await _api.GetUserAsync(id);
        }

        public async Task<OsuUser?> GetUserAsync(string username)
        {
            return await _api.GetUserAsync(username);
        }

        public async Task<OsuBeatmap?> GetBeatmapAsync(long id)
        {
            OsuBeatmap? beatmap = _database.Beatmaps.FindById(id);

            if (beatmap is not null && !beatmap.IsScoreable)
                beatmap = await _api.GetBeatmapAsync(id);

            return beatmap;
        }

        public async Task<OsuBeatmapset?> GetBeatmapsetAsync(long id)
        {
            OsuBeatmapset? beatmapset = _database.Beatmapsets.FindById(id);

            if (beatmapset is not null && !beatmapset.IsScoreable)
                beatmapset = await _api.GetBeatmapsetAsync(id);

            return beatmapset;
        }

        public async Task<OsuBeatmapAttributes?> GetScoreBeatmapAttributesAsync(OsuScore score)
        {
            int mods = score.Mods;
            if (score.Mods == NoMod.NUMBER)
                mods = 0;

            OsuBeatmapAttributes? attributes = _database.BeatmapAttributes
                .FindOne(a => a.Id.BeatmapId == score.Beatmap.Id && a.Id.Mods == mods);
            attributes ??= await _api.GetBeatmapAttributesAsync(score.Beatmap.Id, mods);

            if (attributes is null)
                return null;

            attributes.Id = new BeatmapAttributesKey(score.Beatmap.Id, score.Mods);
            attributes.CopyBeatmapAttributes(score.Beatmap);
            attributes.CalculateAttributesWithMods(ModsConverter.ToMods(mods));
                
            return attributes;
        }

        public async Task<OsuScore?> GetScoreAsync(long id)
        {
            OsuScore? score = _database.Scores.FindById(id);
            score ??= await _api.GetScoreAsync(id);

            return score;
        }

        public async Task<IList<OsuScore>?> GetUserScoresAsync(UserScoreQueryParameters parameters, bool includeBeatmapsAttributes = true)
        {
            OsuUser? user = null;
            if (parameters.UserId is not null)
                user = await GetUserAsync(parameters.UserId.Value);
            else if (parameters.Username is not null)
                user = await GetUserAsync(parameters.Username);

            if (user is null)
                throw new NotImplementedException();

            parameters.UserId = user.Id;

            IList<OsuScore>? scores = await _api.GetUserScoresAsync(parameters);

            if (scores is null)
                return null;

            scores = scores.Where(s => parameters.Mods == AllMods.NUMBER || s.Mods == parameters.Mods)
                .Where(s => parameters.MapsStatusOnly is null || parameters.MapsStatusOnly.Any(b => b == s.Beatmap.Status))
                .Skip(parameters.Offset)
                .Take(parameters.Limit)
                .ToList();
         
            foreach (OsuScore score in scores)
            {
                score.User = user;
                if (includeBeatmapsAttributes)
                {
                    OsuBeatmapAttributes? attributes = await GetScoreBeatmapAttributesAsync(score);
                    if (attributes is null)
                        throw new NotImplementedException();
                    score.BeatmapAttributes = attributes;
                }
            }

            return scores;
        }

        public async Task<OsuScore?> GetUserBeatmapBestScoreAsync(long beatmapId, long userId, bool includeBeatmapsAttributes = true)
        {
            OsuScore? score = await _api.GetUserBeatmapBestScoreAsync(beatmapId, userId);

            if (score is null)
            {
                score = _database.Scores
                       .Include(s => s.Beatmap)
                       .Include(s => s.Beatmapset)
                       .Find(s => s.Beatmap.Id == beatmapId && s.User.Id == userId)
                       .MaxBy(s => s.Score);

                if (score is not null)
                    score.User = await GetUserAsync(userId);
            }

            if (score is not null && includeBeatmapsAttributes)
            {
                OsuBeatmapAttributes? attributes = await GetScoreBeatmapAttributesAsync(score);
                if (attributes is null)
                    throw new NotImplementedException();
                score.BeatmapAttributes = attributes;
            }

            return score;
        }

        public async Task<IList<OsuScore>?> GetUserBeatmapAllScoresAsync(long beatmapId, long userId, bool includeBeatmapsAttributes = true)
        {
            OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId);
            if (beatmap is null)
                throw new NotImplementedException();

            OsuUser? user = await GetUserAsync(userId);
            if (user is null)
                throw new NotImplementedException();

            IList<OsuScore>? scores = null;
            if (beatmap.IsScoreable)
                scores = await _api.GetUserBeatmapAllScoresAsync(beatmapId, userId);
            else
                scores = _database.Scores.Find(s => s.Beatmap.Id == beatmapId && s.User.Id == userId).ToList();

            if (scores is null)
                return null;

            foreach (OsuScore score in scores)
            {
                score.User = user;
                score.Beatmap = beatmap;
                if (includeBeatmapsAttributes)
                {
                    OsuBeatmapAttributes? attributes = await GetScoreBeatmapAttributesAsync(score);
                    if (attributes is null)
                        throw new NotImplementedException();

                    score.BeatmapAttributes = attributes;
                }
            }

            return scores;
        }
    }
}
