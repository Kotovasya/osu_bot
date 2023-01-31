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

namespace osu_bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //BotHandle bot = new BotHandle();
            //await bot.Run();

            BeatmapScore bs = new BeatmapScore()
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
                Rank = "S",
                Beatmap = new()
                {
                    SongName = "MIMI feat. Hatsune Miku - Mizuoto to Curtain",
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
                User = new()
                {
                    Name = "Kotovasya",
                    AvatarUrl = "https://a.ppy.sh/15833700?1673169745.jpeg",
                    CountryCode = "BY",
                    WorldRating = 29345,
                    CountryRating = 101,
                    PP = 6212,
                }
            };
            Image image = new ImageGenerator().CreateFullCard(bs);
            image.Save("TestFullCard.png");
        }
    }
}