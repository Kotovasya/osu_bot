using Newtonsoft.Json.Linq;
using osu_bot.API.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Queries
{
    public abstract class Query<T, U>
        where T : IQueryParameters, new()
    {
        protected OsuAPI API = OsuAPI.Instance;

        private T parameters = new();
        public T Parameters { get => parameters; }
        public string UrlParameter => Parameters.GetQueryString();

        public async Task<U> ExecuteAsync()
        {
            var result = await RunAsync();
            parameters = new();
            return result;
        }

        protected abstract Task<U> RunAsync();
    }
}
