// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace osu_bot.Resources
{
    public static class ResourcesManager
    {
        public static ModsManager ModsManager { get; } = new ModsManager();

        public static MapStatusManager MapStatusManager { get; } = new MapStatusManager();

        public static FontsManager FontsManager { get; } = new FontsManager();

        public static BotStatusManager BotStatusManager { get; } = new BotStatusManager();
    }
}
