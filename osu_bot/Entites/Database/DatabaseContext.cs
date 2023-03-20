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

        private DatabaseContext(string connectionString) => _database = new LiteDatabase(connectionString);

        public ILiteCollection<TelegramUser> TelegramUsers => _database.GetCollection<TelegramUser>();

        public ILiteCollection<ScoreInfo> Scores => _database.GetCollection<ScoreInfo>();

        public ILiteCollection<Request> Requests => _database.GetCollection<Request>();
    }
}
