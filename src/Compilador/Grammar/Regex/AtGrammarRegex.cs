using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class AtGrammarRegex : GrammarRegex
    {
        public AtGrammarRegex(string reference, Location location) : base(location)
        {
            Reference = reference;
        }

        public string Reference { get; set; }
    }
}
