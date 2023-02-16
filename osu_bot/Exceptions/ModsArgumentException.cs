using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Exceptions
{
    public class ModsArgumentException : ArgumentException
    {
        public ModsArgumentException(string? paramName = null)
            : base(paramName)
        {

        }

        public override string Message => 
            ParamName != null 
            ? $"Неизвестный мод {ParamName}" 
            : "Неправильно указаны моды, пример: +DTHD, +PFTDFL, +HR";
    }
}
