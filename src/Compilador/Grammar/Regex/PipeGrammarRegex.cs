using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class PipeGrammarRegex : GrammarRegex
    {
        public IReadOnlyList<GrammarRegex> Alternatives { get; private set; }

        public PipeGrammarRegex(IEnumerable<GrammarRegex> alternatives, Location location) : base(location)
        {
            this.Alternatives = alternatives.ToList().AsReadOnly();
        }
    }
}
