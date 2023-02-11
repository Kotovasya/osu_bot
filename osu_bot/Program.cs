using Newtonsoft.Json;
using osu_bot.Bot;
using osu_bot.Entites;
using osu_bot.Modules;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Formats.Asn1.AsnWriter;
using User = osu_bot.Entites.User;

namespace osu_bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //var str = "16.09.2021 16:59:25";
            //var a = DateTime.ParseExact(str, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            BotHandle bot = new BotHandle();
            await bot.Run();
            //User u = new Entites.User()
            //{
            //    Name = "Kotovasya",
            //    AvatarUrl = "https://a.ppy.sh/15833700?1673169745.jpeg",
            //    CountryCode = "BY",
            //    Accuracy = 98.34f,
            //    WorldRating = 29345,
            //    CountryRating = 101,
            //    PP = 6212,
            //    PlayCount = 88432,
            //    RankHistory = new int[]
            //        {
            //            17321,
            //            17323,
            //            19245,
            //            15000,
            //            14000,
            //            13400,
            //            12400,
            //            13400,
            //            13234,
            //            12900,
            //            12560,
            //            12530,
            //            12129,
            //            11727,
            //            11890,
            //            11545,
            //            11211,
            //            9727,
            //            8000,
            //            8123
            //        }
            //};
            //ScoreInfo bs = new ScoreInfo()
            //{
            //    MaxCombo = 251,
            //    Accuracy = 98.35f,
            //    Score = 1188308,
            //    Date = DateTime.Parse("2022-12-18T12:36:27Z"),
            //    Mods = Mods.DT | Mods.HD,
            //    PP = 270,
            //    CountMisses = 0,
            //    Count300 = 158,
            //    Count100 = 4,
            //    Count50 = 0,
            //    Rank = "SH",
            //    Beatmap = new()
            //    {
            //        Title = "MIMI feat. Hatsune Miku - Mizuoto to Curtain",
            //        DifficultyName = "Insane",
            //        CoverUrl = "https://assets.ppy.sh/beatmaps/968171/covers/cover@2x.jpg?1645788271",
            //        Attributes = new() 
            //        { 
            //            MaxCombo = 251, 
            //            Stars = 6.54f,
            //            CS = 3.6f,
            //            AR = 10.33f,
            //            OD = 10f,
            //            HP = 4.8f,
            //            BPM = 279,
            //            Length = 54,
            //        },
            //        MapperName = "Log Off Now",
            //    },
            //    User = u,
            //};
            //List<ScoreInfo> beatmapScores = new() { bs, bs, bs, bs, bs };
            //ImageGenerator.CreateSmallCard(bs, true).Save("TestSmallCard.png");
            //ImageGenerator.CreateFullCard(bs).Save("TestFullCard.png");
            //ImageGenerator.CreateProfileCard(u).Save("TestProfileCard.png");
            //ImageGenerator.CreateScoresCard(beatmapScores).Save("TestScoresCard.png");
        }
    }
}