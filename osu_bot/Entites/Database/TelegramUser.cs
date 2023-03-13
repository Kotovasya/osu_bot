// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace osu_bot.Entites.Database
{
    public class TelegramUser
    {
        public TelegramUser()
        {
        }

        public TelegramUser(long id, int osuId, string osuName)
        {
            Id = id;
            OsuId = osuId;
            OsuName = osuName;
        }

        public long Id { get; set; }

        public int OsuId { get; set; }

        public string OsuName { get; set; }
    }
}
