// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using LiteDB;

namespace osu_bot.Entites.Database
{
    public class TelegramUser
    {
        public TelegramUser() { }

        public TelegramUser(long id, long chatId, OsuUser osuUser)
        {
            Id = id;
            ChatId = chatId;
            OsuUser = osuUser;
        }

        public long Id { get; set; }

        public long ChatId { get; set; }

        [BsonRef]
        public OsuUser OsuUser { get; set; }
    }
}
