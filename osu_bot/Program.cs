using System.Net.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu_bot.Bot;
using osu_bot.Modules.Converters;
using Telegram.Bot.Types;

namespace osu_bot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            TelegramBot bot = new();
            await bot.RunAsync();
        }
    }
}
