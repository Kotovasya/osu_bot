using osu_bot.API;
using osu_bot.API.Queries;
using osu_bot.Entites;
using osu_bot.Entites.Database;
using osu_bot.Modules;
using SkiaSharp;
using System.Drawing;

namespace osu_bot
{
    internal class Program
    {
        private static readonly BeatmapScoresQuery beatmapScoresQuery = new();
        private static readonly BeatmapInfoQuery beatmapInfoQuery = new();
        private static readonly BeatmapAttributesJsonQuery beatmapAttributesQuery = new();
        private static readonly DatabaseContext Database = DatabaseContext.Instance;

        static async Task Main(string[] args)
        {
            
            //BotHandle bot = new BotHandle();
            //await bot.Run();

            await OsuAPI.Instance.InitalizeAsync();
            int beatmapId = 1385398;

            beatmapInfoQuery.Parameters.BeatmapId = beatmapId;
            var beatmap = await beatmapInfoQuery.ExecuteAsync();
            var mods = ModsConverter.ToMods(Array.Empty<string>());

            beatmapAttributesQuery.Parameters.BeatmapId = beatmapId;
            beatmapAttributesQuery.Parameters.Mods = mods;
            beatmap.Attributes.ParseDifficultyAttributesJson(await beatmapAttributesQuery.ExecuteAsync());

            List<ScoreInfo> result = new();
            var telegramUsers = Database.TelegramUsers.FindAll().ToList();

            foreach (var telegramUser in telegramUsers)
            {
                //После выполнения ExecuteAsync beatmapScoreQuery.Parameters = new()
                beatmapScoresQuery.Parameters.Mods = mods;
                beatmapScoresQuery.Parameters.BeatmapId = beatmapId;
                beatmapScoresQuery.Parameters.Username = telegramUser.OsuName;
                var scores = await beatmapScoresQuery.ExecuteAsync();

                foreach (var score in scores)
                    score.Beatmap = beatmap;

                result.AddRange(scores);
            }

            SKImage image = await CrossplatformImageGenerator.Instance.CreateTableScoresCard(result);
            MemoryStream stream = new MemoryStream();
            image.Encode().SaveTo(stream);
            Image.FromStream(stream).Save("TestTableScores.png");
        }
    }
}
