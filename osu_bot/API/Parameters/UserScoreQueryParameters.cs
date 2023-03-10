// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Entites.Mods;
using osu_bot.Exceptions;
using osu_bot.Modules;

namespace osu_bot.API.Parameters
{
    public class UserScoreQueryParameters
        : IQueryParameters, IParseParameters
    {
        public UserScoreQueryParameters()
        {
            IncludeFails = true;
        }

        public string? Username { get; set; }

        public int UserId { get; set; }

        public IEnumerable<Mod>? Mods { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

        public bool IsRecent { get; set; }

        public bool IncludeFails { get; set; }

        public string GetQueryString() => IsRecent
                ? $"https://osu.ppy.sh/api/v2/users/{UserId}/scores/recent?include_fails={(IncludeFails ? 1 : 0)}&limit=100"
                : $"https://osu.ppy.sh/api/v2/users/{UserId}/scores/best?limit=100";

        public void Parse(string input)
        {
            if (IsRecent)
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
                        Mods = parameterMods;

                        if (result.Length < 2 || result.Length % 2 != 0)
                        {
                            throw new ModsArgumentException();
                        }

                        IEnumerable<string> modsStrings = result.ToUpper().Split(2);
                        foreach (string modString in modsStrings)
                        {
                            parameterMods.Add(ModsConverter.ToMod(modString));
                        }
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
