using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class AlternativeRegex : Regex
    {
        public IReadOnlyList<Regex> Alternatives { get; private set; }

        public AlternativeRegex(IEnumerable<Regex> alternatives, Location location) : base(location)
        {
            this.Alternatives = alternatives.ToList().AsReadOnly();
        }
        private AlternativeRegex()
        {

        }

        public override string ToString()
        {
            return "(" + string.Join(" | ", Alternatives) + ")";
        }

        public override IEnumerable<Regex> Children => Alternatives;

        public override bool Lex(GrammarDefinition grammar, string source, ref int offset)
        {
            var original = offset;
            foreach (var alt in Alternatives)
            {
                alt.Lex(grammar, source, ref offset);
                if (offset != original)
                    return true;
            }
            return false;
        }
    }
}
