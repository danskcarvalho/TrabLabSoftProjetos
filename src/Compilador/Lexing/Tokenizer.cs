using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Lexing
{
    static class Tokenizer
    {
        public static Token[] Lex(string source)
        {
            List<Token> result = new List<Token>();

            var mapping = new LineColumnMapping(source);
            ILexer[] lexers = new ILexer[]
            {
                new CommentLexer(mapping, source),
                new EofLexer(mapping, source),
                new EscapeSequenceLexer(mapping, source),
                new IdentifierLexer(mapping, source),
                new OperatorLexer(mapping, source),
                new StringLiteralLexer(mapping, source),
                new WhitespaceLexer(mapping, source)
            };

            int offset = 0;
            Token lastToken = null;
            do
            {
                for (int i = 0; i < lexers.Length; i++)
                {
                    lastToken = lexers[i].TryLex(ref offset);
                    if (lastToken != null)
                        break;
                }
                if (lastToken == null)
                    throw new GrammarException(mapping.GetLocation(offset), $"caractere inesperado {source[offset]}");

                result.Add(lastToken);
            }
            while (lastToken.Type != TokenType.Eof);

            return result.Where(x => x.Type != TokenType.Whitespace && x.Type != TokenType.Comment).ToArray();
        }
    }
}
