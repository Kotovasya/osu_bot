// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace osu_bot.Entites.Database
{
    public class TelegramUser
    {
        public TelegramUser()
        {
        }

        public TelegramUser(long id, int osuId, string osuName, long chatId)
        {
            Id = id;
            OsuId = osuId;
            OsuName = osuName;
            ChatId = chatId;
        }

        public long ChatId { get; set; }

        public long Id { get; set; }

        public int OsuId { get; set; }

        public string OsuName { get; set; }
    }
}
