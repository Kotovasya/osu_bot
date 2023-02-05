using Newtonsoft.Json;
using osu_bot.Bot;
using osu_bot.Entites;
using osu_bot.Images;
using System.Drawing;
using System.Drawing.Imaging;
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
            //BotHandle bot = new BotHandle();
            //await bot.Run();
            User u = new Entites.User()
            {
                Name = "Kotovasya",
                AvatarUrl = "https://a.ppy.sh/15833700?1673169745.jpeg",
                CountryCode = "BY",
                Accuracy = 98.34,
                WorldRating = 29345,
                CountryRating = 101,
                PP = 6212,
                PlayCount = 88432,
                PlayTime = "50d 12h 39m 8s",
                DateRegistration = "28.12.2020",
                LastOnline = "3 hour ago",
                RankHistory = new int[]
                    {
                        17321,
                        17323,
                        19245,
                        15000,
                        14000,
                        13400,
                        12400,
                        13400,
                        13234,
                        12900,
                        12560,
                        12530,
                        12129,
                        11727,
                        11890,
                        11545,
                        11211,
                        9727,
                        8000,
                        8123
                    }
            };
            ScoreInfo bs = new ScoreInfo()
            {
                MaxCombo = 251,
                Accuracy = 98.35,
                Score = 1188308,
                Date = DateTime.Parse("2022-12-18T12:36:27Z"),
                Mods = Mods.DT | Mods.HD,
                PP = 270,
                CountMisses = 0,
                Count300 = 158,
                Count100 = 4,
                Count50 = 0,
                Complition = 100.00,
                Rank = "SH",
                Beatmap = new()
                {
                    Title = "MIMI feat. Hatsune Miku - Mizuoto to Curtain",
                    DifficultyName = "Insane",
                    CoverUrl = "https://assets.ppy.sh/beatmaps/968171/covers/cover@2x.jpg?1645788271",
                    Attributes = new() 
                    { 
                        MaxCombo = 251, 
                        Stars = 6.54,
                        CS = 3.6,
                        AR = 10.33,
                        OD = 10,
                        HP = 4.8,
                        BPM = 279,
                        Length = 54,
                    },
                    Mapper = new()
                    {
                        Name = "Log Off Now"
                    }
                },
                User = u,
            };
            ImageGenerator ig = new ImageGenerator();
            List<ScoreInfo> beatmapScores = new() { bs, bs, bs, bs, bs };
            //ig.CreateSmallCard(bs).Save("TestSmallCard.png");
            //ig.CreateFullCard(bs).Save("TestFullCard.png");
            //ig.CreateProfileCard(u).Save("TestProfileCard.png");
            ig.CreateScoresCard(beatmapScores).Save("TestScoresCard.png");
        }
    }
}