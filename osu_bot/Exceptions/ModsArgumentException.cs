using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Exceptions
{
    public class ModsArgumentException : Exception
    {
        public override string Message => "Неправильно указаны моды, пример: +DTHD, +NM, +HRNF";
    }
}
