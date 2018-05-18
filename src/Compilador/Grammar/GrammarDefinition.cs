using Compilador.Lalr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class GrammarDefinition : GrammarProductionDatabase
    {
        public GrammarDefinition(
            IEnumerable<GrammarProduction> productions, 
            NonterminalSymbol startingSymbol) : base(productions, startingSymbol)
        {
        }

        public GrammarDefinition(
            bool isCaseSensitive, 
            string lineComment, 
            string startBlockComment,
            string endBlockComment, 
            IEnumerable<GrammarRegexDefinition> regexDefinitions, 
            IEnumerable<CharsetDefinition> charsetDefinitions, 
            IEnumerable<string> keywords,
            IEnumerable<GrammarProduction> productions,
            NonterminalSymbol startingSymbol) : base(productions, startingSymbol)
        {
            IsCaseSensitive = isCaseSensitive;
            LineComment = lineComment;
            StartBlockComment = startBlockComment;
            EndBlockComment = endBlockComment;
            RegexDefinitions = regexDefinitions.ToList().AsReadOnly();
            CharsetDefinitions = charsetDefinitions.ToList().AsReadOnly();
            Keywords = keywords.ToList().AsReadOnly();
        }

        public bool IsCaseSensitive { get; private set; }
        public string LineComment { get; private set; }
        public string StartBlockComment { get; private set; }
        public string EndBlockComment { get; private set; }
        public IReadOnlyList<GrammarRegexDefinition> RegexDefinitions { get; private set; }
        public IReadOnlyList<CharsetDefinition> CharsetDefinitions { get; private set; }
        public IReadOnlyList<string> Keywords { get; private set; }
    }
}
