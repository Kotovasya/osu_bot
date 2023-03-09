using osu_bot.Bot;

namespace osu_bot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            BotHandle bot = new();
            await bot.Run();
        }
    }
}
