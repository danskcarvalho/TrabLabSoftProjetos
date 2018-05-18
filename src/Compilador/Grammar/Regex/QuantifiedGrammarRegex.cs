using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public enum QuantificationType
    {
        ZeroOrMore,
        OneOrMore,
        ZeroOrOne
    }
    public class QuantifiedGrammarRegex : GrammarRegex
    {
        public QuantificationType Type { get; private set; }
        public GrammarRegex Quantified { get; private set; }

        public QuantifiedGrammarRegex(QuantificationType type, GrammarRegex quantified, Location location) : base(location)
        {
            this.Type = type;
            this.Quantified = quantified;
        }
    }
}
