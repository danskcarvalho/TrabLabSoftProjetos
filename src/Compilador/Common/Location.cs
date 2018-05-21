using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Common
{
    [Serializable]
    public struct Location
    {
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Location(int line, int column)
        {
            this.Line = line;
            this.Column = column;
        }
    }
}
