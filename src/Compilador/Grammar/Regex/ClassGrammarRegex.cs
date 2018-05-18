using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class ClassGrammarRegex : GrammarRegex
    {
        public IReadOnlyList<ClassGrammarRegexElement> Elements { get; private set; }
        public ClassGrammarRegex(IEnumerable<ClassGrammarRegexElement> elements, Location location) : base(location)
        {
            this.Elements = elements.ToList().AsReadOnly();
        }
    }

    public class ClassGrammarRegexElement
    {
        public GrammarRegex Start { get; private set; }
        public GrammarRegex End { get; private set; }

        public ClassGrammarRegexElement(GrammarRegex start)
        {
            this.Start = start;
        }
        public ClassGrammarRegexElement(GrammarRegex start, GrammarRegex end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
