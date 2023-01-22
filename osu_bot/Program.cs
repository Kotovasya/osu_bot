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

            BeatmapScoreImage bs = new BeatmapScoreImage()
            {
                Score = new()
                {
                    MaxCombo = 540,
                    Accuracy = 99.45,
                    Date = DateTime.Parse("2022-12-18T12:36:27Z"),
                    Mods = Mods.DT | Mods.HD,
                    PP = 340,
                    CountMisses = 0,
                    Count300 = 411,
                    Count100 = 4,
                    Count50 = 0,
                    Beatmap = new()
                    {
                        SongName = "MIMI feat. Hatsune Miku - Mizuoto to Curtain",
                        DifficultyName = "Insane",
                        CoverUrl = "https://assets.ppy.sh/beatmaps/968171/covers/cover@2x.jpg?1645788271",
                        Attributes = new() { MaxCombo = 540, Stars = 6.54 }
                    },
                    User = new()
                    {
                        Name = "Kotovasya",   
                    }
                }
            };
            Image image = bs.CreateSmallCard();
            image.Save("Test.png");
        }
    }
}