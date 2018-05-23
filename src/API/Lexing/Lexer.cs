using Compilador.Common;
using Compilador.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Lexing
{
    public static class Lexer
    {
        public static IEnumerable<Token> Lex(GrammarDefinition definition, string source)
        {
            var lexers = GetLexers(definition);
            var bySize = new List<Token>();
            int offset = 0;
            var mapping = new LineColumnMapping(source);
            while(offset < source.Length)
            {
                bySize.Clear();
                var original = offset;
                foreach (var item in lexers)
                {
                    var tk = item.TryLex(definition, mapping, source, ref offset);
                    if(tk != null)
                    {
                        offset = original;
                        bySize.Add(tk);
                    }
                }

                if(bySize.Count == 0)
                {
                    throw new GrammarException(mapping.GetLocation(offset), $"erro do analisador léxico próximo de '{source[offset]}'");
                }
                else
                {
                    var maxSize = bySize.Max(x => x.Source.Length);
                    var tk = bySize.Where(x => x.Source.Length == maxSize).First();
                    if (tk.Name != Token.WhitespaceName)
                        yield return tk;
                    offset += tk.Source.Length;
                }
            }

            yield return new Token(Token.EofName, string.Empty, mapping.GetLocation(source.Length));
        }
        private static List<ILexer> GetLexers(GrammarDefinition definition)
        {
            List<ILexer> lexers = new List<ILexer>();
            foreach (var item in definition.Keywords)
            {
                lexers.Add(new KeywordLexer(item));
            }
            foreach (var item in definition.RegexDefinitions)
            {
                lexers.Add(new RegexLexer(item));
            }
            lexers.Add(new WhitespaceLexer());
            return lexers;
        }
    }
}
