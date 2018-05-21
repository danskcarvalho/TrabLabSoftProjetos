using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class SequentialRegex : Regex
    {
        public IReadOnlyList<Regex> Sequences { get; private set; }

        public SequentialRegex(IEnumerable<Regex> sequences, Location location) : base(location)
        {
            this.Sequences = sequences.ToList().AsReadOnly();
        }

        public override string ToString()
        {
            return "(" + string.Join(" ", Sequences) + ")";
        }
    }
}
