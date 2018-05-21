using Compilador.Grammar;
using Compilador.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Parsing
{
    public static class Parser
    {
        private static Node InternalParse(string source)
        {
            var tokens = Tokenizer.Lex(source);
            var parserBase = new ParserBase(tokens);
            return parserBase.Parse();
        }

        public static GrammarDefinition Parse(string source)
        {
            return GrammarGeneration.Build(InternalParse(source));
        }
    }
}
