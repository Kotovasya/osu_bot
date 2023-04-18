// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using osu_bot.API.Handlers;
using osu_bot.Bot;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using osu_bot.Modules.Converters;
using osu_bot.Modules.OsuFiles;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.API
{
    public class OsuService
    {
        public static OsuService Instance { get; } = new();

        private OsuService() { }

        private readonly OsuAPI _api = OsuAPI.Instance;
        private readonly DatabaseContext _database = DatabaseContext.Instance;

        private List<IHandler<IList<OsuScore>>> _scoresHandlers = new();

        public async Task InitalizeAsync(TelegramBot bot)
        {
            await _api.InitalizeAsync();
            _telegramBot = bot;
            _scoresHandlers = new()
            {
                new RequestsHandler(bot),
                new ScoresDatabaseHandler()
            };
        }

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
            OsuBeatmap? beatmap = _database.Beatmaps
                .Include(b => b.Beatmapset)
                .FindById(id);

            if (beatmap is null || !beatmap.IsScoreable)
                beatmap = await _api.GetBeatmapAsync(id);

            return beatmap;
        }

        public async Task<OsuBeatmap?> GetBeatmapAsync(string beatmapHash)
        {
            OsuBeatmap? beatmap = _database.Beatmaps
                .Include(b => b.Beatmapset)
                .FindOne(b => b.Hash == beatmapHash);

            if (beatmap is null || !beatmap.IsScoreable)
                beatmap = await _api.GetBeatmapAsync(beatmapHash);

            return beatmap;
        }

        public async Task<OsuBeatmapset?> GetBeatmapsetAsync(long id)
        {
            OsuBeatmapset? beatmapset = _database.Beatmapsets.FindById(id);

            if (beatmapset is null || !beatmapset.IsScoreable)
                beatmapset = await _api.GetBeatmapsetAsync(id);

            return beatmapset;
        }

        public async Task<OsuBeatmapAttributes?> GetBeatmapAttributesAsync(OsuBeatmap beatmap, int mods)
        {
            OsuBeatmapAttributes? attributes = _database.BeatmapAttributes
                .FindOne(a => a.Id.BeatmapId == beatmap.Id && a.Id.Mods == mods);

            if (attributes is null)
            {
                attributes = await _api.GetBeatmapAttributesAsync(beatmap.Id, mods);

                if (attributes is null)
                    return null;

                attributes.Id = new BeatmapAttributesKey(beatmap.Id, mods);
                attributes.CopyBeatmapAttributes(beatmap);
                attributes.CalculateAttributesWithMods(ModsConverter.ToMods(mods));

                if (beatmap.Status is OsuBeatmapStatus.Ranked or OsuBeatmapStatus.Qualified or OsuBeatmapStatus.Loved)
                    _database.BeatmapAttributes.Insert(attributes);
            }

            return attributes;
        }

        public async Task<OsuBeatmapAttributes?> GetScoreBeatmapAttributesAsync(OsuScore score)
        {
            return await GetBeatmapAttributesAsync(score.Beatmap, score.Mods);
        }

        public async Task<OsuScore?> GetScoreAsync(long id)
        {
            OsuScore? score = _database.Scores.FindById(id);
            score ??= await _api.GetScoreAsync(id);

            if (score is null)
                return null;

            OsuUser? user = await GetUserAsync(score.User.Id);
            if (user is null)
                throw new NotImplementedException();

            score.User = user;

            if (score.Beatmapset is null)
            {
                OsuBeatmapset? beatmapset = await _api.GetBeatmapsetAsync(score.Beatmap.BeatmapsetId);
                if (beatmapset is null)
                    throw new NotImplementedException();
                score.Beatmapset = beatmapset;
            }

            OsuBeatmapAttributes? attributes = await GetScoreBeatmapAttributesAsync(score);
            if (attributes is null)
                throw new NotImplementedException();
            score.BeatmapAttributes = attributes;

            _scoresHandlers.ForEach(async h => await h.HandlingAsync(new List<OsuScore>() { score }));

            return score;
        }

        public async Task<IList<OsuScore>?> GetUserScoresAsync(UserScoreQueryParameters parameters)
        {
            OsuUser? user = null;
            if (parameters.UserId is not null)
                user = await GetUserAsync(parameters.UserId.Value);
            else if (parameters.Username is not null)
                user = await GetUserAsync(parameters.Username);

            if (user is null)
                throw new UserNotFoundException(parameters.Username);

            parameters.UserId = user.Id;

            IList<OsuScore>? scores = await _api.GetUserScoresAsync(parameters);

            if (scores is null)
                return null;

            scores = scores.Where(s => parameters.Mods == AllMods.NUMBER || s.Mods == parameters.Mods)
                .Skip(parameters.Offset)
                .Take(parameters.Limit)
                .ToList();
         
            foreach (OsuScore score in scores)
            {
                score.User = user;
                OsuBeatmapAttributes? attributes = await GetScoreBeatmapAttributesAsync(score);
                if (attributes is null)
                    throw new NotImplementedException();
                score.BeatmapAttributes = attributes;
            }

            _scoresHandlers.ForEach(async c => await c.HandlingAsync(scores));

            return scores;
        }

        public async Task<OsuScore?> GetUserBeatmapBestScoreAsync(long beatmapId, long userId)
        {
            OsuScore? score = await _api.GetUserBeatmapBestScoreAsync(beatmapId, userId);

            if (score is null)
            {
                score = _database.Scores
                       .Include(s => s.Beatmap)
                       .Include(s => s.Beatmapset)
                       .Find(s => s.Beatmap.Id == beatmapId && s.User.Id == userId)
                       .MaxBy(s => s.Score);
            }

            if (score is null)
                return null;

            OsuUser? user = await GetUserAsync(userId);
            if (user is null)
                throw new NotImplementedException();

            score.User = user;

            if (score.Beatmapset is null)
            {
                OsuBeatmapset? beatmapset = await _api.GetBeatmapsetAsync(score.Beatmap.BeatmapsetId);
                if (beatmapset is null)
                    throw new NotImplementedException();

                score.Beatmapset = beatmapset;
            }


            OsuBeatmapAttributes? attributes = await GetScoreBeatmapAttributesAsync(score);
            if (attributes is null)
                throw new NotImplementedException();
            score.BeatmapAttributes = attributes;

            _scoresHandlers.ForEach(async h => await h.HandlingAsync(new List<OsuScore>() { score }));

            return score;
        }

        public async Task<IList<OsuScore>?> GetUserBeatmapAllScoresAsync(long beatmapId, long userId, bool includeBeatmapAttributes = true)
        {
            OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId);
            if (beatmap is null)
                throw new BeatmapNotFoundException(beatmapId);

            OsuUser? user = await GetUserAsync(userId);
            if (user is null)
                throw new UserNotFoundException(userId.ToString());

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
                score.Beatmapset = beatmap.Beatmapset;
                OsuBeatmapAttributes? attributes = await GetScoreBeatmapAttributesAsync(score);
                if (attributes is null)
                    throw new NotImplementedException();
                score.BeatmapAttributes = attributes;
            }

            _scoresHandlers.ForEach(async h => await h.HandlingAsync(scores));

            return scores;
        }

        public async Task<MemoryStream> GetReplayDataAsync(string hash)
        {
            using MemoryStream replayDataStream = new();

            if (long.TryParse(hash, out long scoreId))
            {
                IEnumerable<LiteFileInfo<string>> replays = _database.FileStorage.Find(f => f.Filename == hash);
                if (replays.Any())
                    _database.FileStorage.Download(replays.First().Id, replayDataStream);
                else
                {
                    string? replayData = await _api.GetReplayData(scoreId);
                    if (replayData is not null)
                    {
                        using StreamWriter writer = new(replayDataStream);
                        await writer.WriteAsync(replayData);
                        replayDataStream.Position = 0;
                        OsuReplay replay = ReplayReader.FromStream(replayDataStream);
                        if (!_database.FileStorage.Exists(replay.ReplayHash))
                            _database.FileStorage.Upload(replay.ReplayHash, $"{scoreId}.osr", replayDataStream);

                        if (!_database.Replays.Exists(replay.ReplayHash))
                            _database.Replays.Insert(new ReplayUpload(replay.ReplayHash, replay.OnlineScoreId));
                    }
                }
            }
            else if (_database.FileStorage.Exists(hash))
                _database.FileStorage.Download(hash, replayDataStream);

            return replayDataStream;
        }
    }
}
