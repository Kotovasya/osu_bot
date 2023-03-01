using Newtonsoft.Json;
using osu_bot.Bot;
using osu_bot.Entites;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using osu_bot.Modules;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
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

            User u = new User()
            {
                Name = "Kotovasya",
                AvatarUrl = "https://a.ppy.sh/15833700?1673169745.jpeg",
                CountryCode = "BY",
                WorldRating = 29345,
                CountryRating = 101,
                PP = 6212,
                PlayCount = 88432,
                PlayTime = TimeSpan.FromHours(1500),
                DateRegistration = DateTime.Now,
                LastOnline = DateTime.Now,
            };
            ScoreInfo score = new ScoreInfo()
            {
                MaxCombo = 251,
                Accuracy = 98.35f,
                Score = 1188308,
                Date = DateTime.Parse("2022-12-18T12:36:27Z"),
                Mods = new Mod[] { new ModDoubleTime(), new ModHidden() },
                PP = 270,
                CountMisses = 0,
                Count300 = 158,
                Count100 = 4,
                Count50 = 0,
                Rank = "S",
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
                    MapperName = "Log Off Now"
                },
                User = u,
            };

            var image = CrossplatformImageGenerator.CreateSmallCard(score, true);
            var stream = new MemoryStream();
            image.Encode().SaveTo(stream);
            Image.FromStream(stream).Save("TestSmallCard.png");
            ;
        }
    }
}