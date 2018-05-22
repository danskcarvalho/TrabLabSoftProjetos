using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public abstract class CharsetExpression
    {
        public Location Location { get; private set; }

        public CharsetExpression(Location location)
        {
            this.Location = location;
        }
        protected CharsetExpression() { }

        public abstract bool Contains(GrammarDefinition grammar, char c);
    }
}
