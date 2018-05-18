using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class CharsetRegexGrammar : GrammarRegex
    {
        public string CharsetName { get; private set; }
        public CharsetRegexGrammar(string charsetName, Location location) : base(location)
        {
            this.CharsetName = charsetName;
        }
    }
}
