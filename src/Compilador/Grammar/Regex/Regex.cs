using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public abstract class Regex
    {
        public Location Location { get; private set; }

        public Regex(Location location)
        {
            this.Location = location;
        }
        protected Regex()
        {

        }

        public abstract IEnumerable<Regex> Children { get; }

        public abstract bool Lex(GrammarDefinition grammar, string source, ref int offset);
    }
}
