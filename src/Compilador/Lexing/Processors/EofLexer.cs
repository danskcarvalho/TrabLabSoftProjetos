using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilador.Common;

namespace Compilador.Lexing
{
    class EofLexer : BaseLexer
    {
        public EofLexer(LineColumnMapping mapping, string source) : base(mapping, source)
        {
        }

        public override Token TryLex(ref int offset)
        {
            if (CharAt(offset) == '\0')
            {
                offset++;
                return new Token(null, TokenType.Eof, GetLocation(offset - 1));
            }
            else
                return null;
        }
    }
}
