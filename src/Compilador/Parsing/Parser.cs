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
        public static Node Parse(string source)
        {
            var tokens = Tokenizer.Lex(source);
            var parserBase = new ParserBase(tokens);
            return parserBase.Parse();
        }
    }
}
