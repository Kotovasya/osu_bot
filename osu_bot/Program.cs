using Newtonsoft.Json;
using osu_bot.API;
using osu_bot.API.Queries;
using osu_bot.Bot;
using osu_bot.Bot.Commands;
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

            await OsuAPI.Instance.InitalizeAsync();
            UserScoresQuery query = new();
            query.Parameters.Parse("+2 +DTHD Kotovasya");
            List<ScoreInfo> scores = await query.ExecuteAsync();
            //SKImage imageSmall = await CrossplatformImageGenerator.Instance.CreateScoresCard(scores);
            SKImage imageFull = await CrossplatformImageGenerator.Instance.CreateFullCardAsync(scores.First());
            var stream = new MemoryStream();

            imageFull.Encode().SaveTo(stream);
            Image.FromStream(stream).Save("TestFullCard.png");
            //await stream.FlushAsync();
            //stream.Position = 0;

            //imageSmall.Encode().SaveTo(stream);
            //Image.FromStream(stream).Save("TestSmallCard.png");
        }
    }
}
