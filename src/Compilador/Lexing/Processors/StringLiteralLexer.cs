using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilador.Common;

namespace Compilador.Lexing
{
    public class StringLiteralLexer : BaseLexer
    {
        public StringLiteralLexer(LineColumnMapping mapping, string source) : base(mapping, source)
        {
        }

        public override Token TryLex(ref int offset)
        {
            if (CharAt(offset) == '\'' || CharAt(offset) == '"')
            {
                int start = offset;
                var ending = CharAt(offset);
                offset++;

                while (CharAt(offset) != ending && CharAt(offset) != '\0' && CharAt(offset) != '\n')
                {
                    if (CharAt(offset) == '\\' && CharAt(offset + 1) != '\0' && CharAt(offset + 1) != '\n')
                        offset++;

                    offset++;
                }

                if (CharAt(offset) == '\0' || CharAt(offset) == '\n')
                    throw new GrammarException(GetLocation(offset), "string não finalizada");

                var value = GetValue(GetLocation(start + 1), start + 1, offset - 1);
                offset++;
                return new Token(TokenType.StringLiteral, value, GetLocation(start));
            }
            else
                return null;
        }

        private string GetValue(Location location, int start, int end)
        {
            var val = Substring(start, end);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < val.Length; i++)
            {
                if (val[i] == '\\' && i == (val.Length - 1))
                    throw new GrammarException(new Location(location.Line, location.Column + i), "sequência de escape inválida");
                else if (val[i] == '\\')
                {
                    builder.Append(GetEscapeValue(val[i + 1]));
                    i++;
                }
                else
                    builder.Append(val[i]);
            }
            return builder.ToString();
        }

        private string GetEscapeValue(char next)
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
