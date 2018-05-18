using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class LiteralRegex : Regex
    {
        public string Literal { get; private set; }
        
        public LiteralRegex(string literal, Location location) : base(location)
        {
            this.Literal = literal;
        }
    }
}
