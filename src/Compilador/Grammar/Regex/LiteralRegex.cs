using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class LiteralRegex : Regex
    {
        public string Literal { get; private set; }

        public override IEnumerable<Regex> Children => new List<Regex>();

        public LiteralRegex(string literal, Location location) : base(location)
        {
            this.Literal = literal;
        }
        private LiteralRegex()
        {

        }
        

        public override string ToString()
        {
            return "'" + Literal + "'";
        }
    }
}
