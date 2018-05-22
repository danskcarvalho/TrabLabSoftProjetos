using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilador.Common;

namespace Compilador.Lexing
{
    class OperatorLexer : BaseLexer
    {
        public OperatorLexer(LineColumnMapping mapping, string source) : base(mapping, source)
        {
        }

        public override Token TryLex(ref int offset)
        {
            var op = GetOperator(offset);
            if (op != null)
                offset++;
            return op;
        }

        private Token GetOperator(int offset)
        {
            var current = CharAt(offset);
            switch (current)
            {
                case '/':
                    return new Token("/", TokenType.DivOperator, GetLocation(offset));
                case '=':
                    return new Token("=", TokenType.EqOperator, GetLocation(offset));
                case '|':
                    return new Token("|", TokenType.PipeOperator, GetLocation(offset));
                case '<':
                    return new Token("<", TokenType.LtOperator, GetLocation(offset));
                case '>':
                    return new Token(">", TokenType.GtOperator, GetLocation(offset));
                case '[':
                    return new Token("[", TokenType.OSqrOperator, GetLocation(offset));
                case ']':
                    return new Token("]", TokenType.ESqrOperator, GetLocation(offset));
                case '-':
                    return new Token("-", TokenType.MinusOperator, GetLocation(offset));
                case '*':
                    return new Token("*", TokenType.MultOperator, GetLocation(offset));
                case '+':
                    return new Token("+", TokenType.PlusOperator, GetLocation(offset));
                case '(':
                    return new Token("(", TokenType.ORoundOperator, GetLocation(offset));
                case ')':
                    return new Token(")", TokenType.ERoundOperator, GetLocation(offset));
                case '?':
                    return new Token("?", TokenType.QtOperator, GetLocation(offset));
                case '{':
                    return new Token("{", TokenType.OCurlyOperator, GetLocation(offset));
                case '}':
                    return new Token("}", TokenType.ECurlyOperator, GetLocation(offset));
                case '@':
                    return new Token("@", TokenType.AtOperator, GetLocation(offset));
                default:
                    return null;
            }
        }
    }
}
