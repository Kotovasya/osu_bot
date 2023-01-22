using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Queries
{
    public abstract class Query<T>
    {
        public string UrlParameter { get; protected set; }

        public abstract Task<T> ExecuteAsync(OsuAPI api);
    }
}
