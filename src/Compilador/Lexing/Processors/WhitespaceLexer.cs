using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilador.Common;

namespace Compilador.Lexing
{
    public class WhitespaceLexer : BaseLexer
    {
        public WhitespaceLexer(LineColumnMapping mapping, string source) : base(mapping, source)
        {
        }

        public override Token TryLex(ref int offset)
        {
            if (char.IsWhiteSpace(CharAt(offset)))
            {
                int start = offset;
                while (char.IsWhiteSpace(CharAt(offset)))
                    offset++;
                
                return new Token(TokenType.Whitespace, GetLocation(start));
            }
            else
                return null;
        }
    }
}
