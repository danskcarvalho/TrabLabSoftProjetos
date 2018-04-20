using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilador.Common;

namespace Compilador.Lexing
{
    public class EscapeSequenceLexer : BaseLexer
    {
        public EscapeSequenceLexer(LineColumnMapping mapping, string source) : base(mapping, source)
        {
        }

        public override Token TryLex(ref int offset)
        {
            if (CharAt(offset) == '\\')
            {
                offset += 2;
                var next = CharAt(offset - 1);
                if (next == '\0' || next == '\n')
                    throw new GrammarException(GetLocation(offset - 1), $"sequência de escape inválida");
                var value = GetValue(next);
                return new Token($"\\{CharAt(offset - 1)}", TokenType.EscapeSequence, value, GetLocation(offset - 1));
            }
            else
                return null;
        }

        private string GetValue(char next)
        {
            switch (next)
            {
                case '\'':
                    return "'";
                case '"':
                    return "\"";
                case '\\':
                    return "\\";
                case '0':
                    return "\0";
                case 'a':
                    return "\a";
                case 'b':
                    return "\b";
                case 'f':
                    return "\f";
                case 'n':
                    return "\n";
                case 'r':
                    return "\r";
                case 't':
                    return "\t";
                case 'v':
                    return "\v";
                default:
                    return next.ToString();
            }
        }
    }
}
