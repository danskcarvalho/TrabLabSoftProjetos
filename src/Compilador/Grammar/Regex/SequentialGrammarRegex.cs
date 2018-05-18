using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class SequentialGrammarRegex : GrammarRegex
    {
        public IReadOnlyList<GrammarRegex> Sequences { get; private set; }

        public SequentialGrammarRegex(IEnumerable<GrammarRegex> sequences, Location location) : base(location)
        {
            this.Sequences = sequences.ToList().AsReadOnly();
        }
    }
}
