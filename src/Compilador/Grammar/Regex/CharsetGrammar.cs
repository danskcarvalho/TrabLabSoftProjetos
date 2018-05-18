using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class CharsetGrammar : Regex
    {
        public string CharsetName { get; private set; }
        public CharsetGrammar(string charsetName, Location location) : base(location)
        {
            this.CharsetName = charsetName;
        }
    }
}
