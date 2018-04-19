using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Common
{

    [Serializable]
    public class GrammarException : Exception
    {
        public Location Location { get; private set; }

        public GrammarException(Location location, string diagnostic)
            : base(GetMessage(location, diagnostic))
        {
            this.Location = location;
        }

        private static string GetMessage(Location location, string diagnostic)
        {
            return $"{location.Line + 1}:{location.Column + 1}: {diagnostic}";
        }
    }
}
