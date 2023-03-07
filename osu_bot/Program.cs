using osu_bot.API;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Modules;
using SkiaSharp;
using System.Drawing;

namespace osu_bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            //BotHandle bot = new BotHandle();
            //await bot.Run();

            await OsuAPI.Instance.InitalizeAsync();
            UserScoresQuery query = new();
            query.Parameters.Parse("+2 +DTHD Kotovasya");
            List<ScoreInfo> scores = await query.ExecuteAsync();
            //SKImage imageSmall = await CrossplatformImageGenerator.Instance.CreateScoresCard(scores);
            SKImage imageFull = await CrossplatformImageGenerator.Instance.CreateFullCardAsync(scores.First());
            var stream = new MemoryStream();

            imageFull.Encode().SaveTo(stream);
            Image.FromStream(stream).Save("TestFullCard.png");
            await stream.FlushAsync();
            stream.Position = 0;

            //imageSmall.Encode().SaveTo(stream);
            //Image.FromStream(stream).Save("TestSmallCard.png");
        }
    }
}
