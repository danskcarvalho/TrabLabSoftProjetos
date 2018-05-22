using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilador.Common;

namespace Compilador.Lexing
{
    class CommentLexer : BaseLexer
    {
        public CommentLexer(LineColumnMapping mapping, string source) : base(mapping, source)
        {
        }

        public override Token TryLex(ref int offset)
        {
            if (CharAt(offset) == '#')
            {
                int start = offset;
                while (CharAt(offset) != '\n' && CharAt(offset) != '\0')
                    offset++;
                return new Token(null, TokenType.Comment, GetLocation(start));
            }
            else
                return null;
        }
    }
}
