// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using osu_bot.Bot.Callbacks;
using osu_bot.Entites;
using SkiaSharp;
using Telegram.Bot.Types.ReplyMarkups;

namespace osu_bot.Modules
{
    public static class Extensions
    {
        public static string Separate(this int number, string separator)
        {
            NumberFormatInfo format = new()
            {
                NumberGroupSeparator = separator
            };
            return number.ToString("N", format)[..^3];
        }

        public static IEnumerable<string> Split(this string str, int n)
        {
            if (string.IsNullOrEmpty(str) || n < 1)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < str.Length; i += n)
            {
                yield return str.Substring(i, Math.Min(n, str.Length - i));
            }
        }

        public static SKPaint SetColor(this SKPaint paint, SKColor color)
        {
            paint.Color = color;
            return paint;
        }

        public static SKPaint SetSize(this SKPaint paint, float size)
        {
            paint.TextSize = size;
            return paint;
        }

        public static SKPaint SetTypeface(this SKPaint paint, SKTypeface typeface)
        {
            paint.Typeface = typeface;
            return paint;
        }

        public static void DrawText(this SKCanvas canvas, string text, SKRect rect, SKPaint paint, bool multiLine = false)
        {
            float spaceWidth = paint.MeasureText(" ");
            float wordX = rect.Left;
            float wordY = rect.Top;
            foreach (string word in text.Split(' '))
            {
                float wordWidth = paint.MeasureText(word);
                if (wordWidth <= rect.Right - wordX)
                {
                    canvas.DrawText(word, wordX, wordY, paint);
                    wordX += wordWidth + spaceWidth;
                }
                else if (multiLine)
                {
                    wordY += paint.FontSpacing;
                    wordX = rect.Left;
                }
                else
                {
                    int i = 0;
                    string subWord;
                    do
                    {
                        subWord = $"{word[..^++i]}...";
                    }
                    while (wordWidth < rect.Right - wordX || i < word.Length);

                    canvas.DrawText(subWord, wordX, wordY, paint);
                    return;
                }
            }
        }

        public static void DrawAlignText(this SKCanvas canvas, string drawableString, float centerX, float y, SKTextAlign align, SKPaint paint)
        {
            float x = align switch
            {
                SKTextAlign.Left => centerX,
                SKTextAlign.Center => centerX - (paint.MeasureText(drawableString) / 2),
                SKTextAlign.Right => centerX - paint.MeasureText(drawableString),
                _ => centerX,
            };
            canvas.DrawText(drawableString, x, y, paint);
        }

        public static InlineKeyboardMarkup KeyboardMarkupForMap(long beatmapId)
        {
            return new InlineKeyboardMarkup(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "🎯Мой скор", callbackData: $"{MyScoreCallback.DATA} beatmapId{beatmapId}"),
                    InlineKeyboardButton.WithCallbackData(text: "🏆Топ конфы", callbackData: $"{TopConferenceCallback.DATA} beatmapId{beatmapId}"),
                    InlineKeyboardButton.WithCallbackData(text: "📌Реквест", callbackData: $"{RequestCallback.DATA}: {beatmapId} A: {RequestAction.Create} P: 1")
                });
        }
    }
}
