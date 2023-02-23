using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.API.Queries
{
    public interface IQuery<T>
    {
        public abstract string UrlParameter { get; }
        public abstract Task<T> ExecuteAsync(OsuAPI api);
    }
}
