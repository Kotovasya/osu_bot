// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using LiteDB;

namespace osu_bot.Entites.Database
{
    public class DatabaseContext
    {
        private static readonly string connectionString = @"Database.db";

        public static DatabaseContext Instance { get; } = new(connectionString);

        private readonly LiteDatabase db;

        private DatabaseContext(string connectionString) => db = new LiteDatabase(connectionString);

        public ILiteCollection<TelegramUser> TelegramUsers => db.GetCollection<TelegramUser>();

        public ILiteCollection<ScoreInfo> Scores => db.GetCollection<ScoreInfo>();
    }
}
