// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.API.Parameters;

namespace osu_bot.API.Queries
{
    public abstract class Query<T, U>
        where T : IQueryParameters, new()
    {
        protected OsuAPI API = OsuAPI.Instance;

        public T Parameters { get; private set; } = new();
        public string UrlParameter => Parameters.GetQueryString();

        public async Task<U> ExecuteAsync()
        {
            U? result = await RunAsync();
            Parameters = new();
            return result;
        }

        protected abstract Task<U> RunAsync();
    }
}
