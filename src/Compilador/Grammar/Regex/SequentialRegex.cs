using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class SequentialRegex : Regex
    {
        public IReadOnlyList<Regex> Sequences { get; private set; }
        public override IEnumerable<Regex> Children => Sequences;

        public SequentialRegex(IEnumerable<Regex> sequences, Location location) : base(location)
        {
            this.Sequences = sequences.ToList().AsReadOnly();
        }
        private SequentialRegex()
        {

        }

        public override string ToString()
        {
            return "(" + string.Join(" ", Sequences) + ")";
        }
    }
}
