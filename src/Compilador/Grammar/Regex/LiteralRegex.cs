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

        public override bool Lex(GrammarDefinition grammar, string source, ref int offset)
        {
            for (int i = 0; i < Literal.Length; i++)
            {
                if ((offset + i) >= source.Length)
                    return false;

                if (grammar.IsCaseInsensitive)
                {
                    if (char.ToLowerInvariant(source[offset + i]) != char.ToLowerInvariant(Literal[i]) && 
                        char.ToUpperInvariant(source[offset + i]) != char.ToUpperInvariant(Literal[i]))
                        return false;
                }
                else
                {
                    if (source[offset + i] != Literal[i])
                        return false;
                }
            }

            offset += Literal.Length;
            return true;
        }

        public override string ToString()
        {
            return "'" + Literal + "'";
        }
    }
}
