// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Entites;
using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using osu_bot.Modules;
using osu_bot.Modules.Converters;

namespace osu_bot.API
{
    public enum ScoreType
    {
        Best,
        Firsts,
        Recent,
    }

    public class UserScoreQueryParameters
    {
        public UserScoreQueryParameters(ScoreType type, bool includeFails = false)
        {
            Type = type;
            IncludeFails = includeFails;
            Mods = AllMods.NUMBER;
        }

        public string? Username { get; set; }

        public long? UserId { get; set; }

        public int Mods { get; set; }

        public bool IncludeFails { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

        public ScoreType Type { get; set; }

        public IEnumerable<OsuBeatmapStatus>? MapsStatusOnly { get; set; }

        public string GetQueryString() => Type switch
        {
            ScoreType.Best => $"/users/{UserId}/scores/best?limit=100",
            ScoreType.Firsts => $"/users/{UserId}/scores/firsts?limit=100",
            ScoreType.Recent => $"/users/{UserId}/scores/recent?include_fails={ (IncludeFails ? 1 : 0) }&limit=100",
            _ => throw new ArgumentException()
        };

        public void Parse(string input)
        {
            if (Type == ScoreType.Recent)
                Limit = 1;
            else
                Limit = 5;

            int endIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    int startIndex = i;
                    while (input.Length > i && char.IsDigit(input[i]))
                    {
                        i++;
                    }

                    string result = input[startIndex..i];
                    Offset = 0;
                    Limit = int.Parse(result);
                    endIndex = i;
                }

                else if (input[i] == '+')
                {
                    int startIndex = ++i;
                    while (input.Length > i && char.IsLetterOrDigit(input[i]))
                    {
                        i++;
                    }

                    string result = input[startIndex..i];
                    endIndex = i;

                    if (int.TryParse(result, out int number))
                    {
                        Offset = number - 1; ;
                        Limit = 1;
                    }
                    else if (result == "pass")
                    {
                        IncludeFails = false;
                    }
                    else
                    {
                        HashSet<Mod> parameterMods = new();

                        if (result.Length < 2 || result.Length % 2 != 0)
                        {
                            throw new ModsArgumentException();
                        }

                        IEnumerable<string> modsStrings = result.ToUpper().Split(2);
                        foreach (string modString in modsStrings)
                        {
                            Mod? mod = ModsConverter.ToMod(modString);
                            if (mod is null)
                                throw new ModsArgumentException(modString);

                            parameterMods.Add(mod);
                        }

                        Mods = ModsConverter.ToInt(parameterMods);
                    }
                }
            }
            if (input.Length > endIndex)
            {
                Username = input.Substring(endIndex + 1, input.Length - endIndex - 1);
            }
        }
    }
}
