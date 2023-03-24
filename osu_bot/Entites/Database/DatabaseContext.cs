// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using LiteDB;

namespace osu_bot.Entites.Database
{
    public class DatabaseContext
    {
        private static readonly string s_connectionString = @"Database.db";

        public static DatabaseContext Instance { get; } = new(s_connectionString);

        private readonly LiteDatabase _database;

        private DatabaseContext(string connectionString)
        {
            _database = new LiteDatabase(connectionString);
        }

        public ILiteCollection<TelegramUser> TelegramUsers => _database.GetCollection<TelegramUser>();

        public ILiteCollection<OsuUser> OsuUsers => _database.GetCollection<OsuUser>();

        public ILiteCollection<OsuScore> Scores => _database.GetCollection<OsuScore>();

        public ILiteCollection<Request> Requests => _database.GetCollection<Request>();

        public ILiteCollection<OsuBeatmapAttributes> BeatmapAttributes => _database.GetCollection<OsuBeatmapAttributes>();

        public ILiteCollection<OsuBeatmapset> Beatmapsets => _database.GetCollection<OsuBeatmapset>();

        public ILiteCollection<OsuBeatmap> Beatmaps => _database.GetCollection<OsuBeatmap>();
    }
}
