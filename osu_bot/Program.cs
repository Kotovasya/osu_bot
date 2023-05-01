using System.Net.Security;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu_bot.API;
using osu_bot.Bot;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using osu_bot.Modules.Converters;
using Telegram.Bot.Types;

namespace osu_bot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            TelegramBot bot = new();
            OrdrAPI.Instance.ToString();
            await bot.RunAsync();
        }
    }
}
