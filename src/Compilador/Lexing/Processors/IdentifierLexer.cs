using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilador.Common;

namespace Compilador.Lexing
{
    public class IdentifierLexer : BaseLexer
    {
        public IdentifierLexer(LineColumnMapping mapping, string source) : base(mapping, source)
        {
        }

        public override Token TryLex(ref int offset)
        {
            if (char.IsLetterOrDigit(CharAt(offset)))
            {
                int start = offset;
                while (char.IsLetterOrDigit(CharAt(offset)))
                    offset++;

                var value = Substring(start, offset - 1);
                if (value == "option")
                    return new Token("option", TokenType.OptionKeyword, GetLocation(start));
                else
                    return new Token(value, TokenType.Identifier, value, GetLocation(start));
            }
            else
                return null;
        }
    }
}
