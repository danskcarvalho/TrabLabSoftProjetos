using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class ClassRegex : Regex
    {
        public IReadOnlyList<ClassRegexElement> Elements { get; private set; }
        public ClassRegex(IEnumerable<ClassRegexElement> elements, Location location) : base(location)
        {
            this.Elements = elements.ToList().AsReadOnly();
        }
        private ClassRegex()
        {

        }

        public override string ToString()
        {
            return "[" + string.Join(" ", Elements) + "]";
        }

        public override IEnumerable<Regex> Children => new List<Regex>();

        public bool Contains(GrammarDefinition grammar, char c)
        {
            if (grammar.IsCaseInsensitive)
                return ContainsSensitive(char.ToLowerInvariant(c)) || ContainsSensitive(char.ToUpperInvariant(c));
            else
                return ContainsSensitive(c);
        }

        private bool ContainsSensitive(char c)
        {
            foreach (var item in Elements)
            {
                if (item.End != null)
                {
                    if (item.Start.Literal.Contains(c))
                        return true;
                }
                else
                {
                    var lit = item.Start;
                    var c1 = lit.Literal[lit.Literal.Length - 1];
                    var c2 = item.End.Literal[0];
                    if (lit.Literal.Contains(c))
                        return true;
                    if (item.End.Literal.Contains(c))
                        return true;
                    if (c >= c1 && c <= c2)
                        return true;
                }
            }
            return false;
        }

        public override bool Lex(GrammarDefinition grammar, string source, ref int offset)
        {
            if (Contains(grammar, source[offset]))
            {
                offset++;
                return true;
            }
            else
                return false;
        }
    }
    [Serializable]
    public class ClassRegexElement
    {
        public LiteralRegex Start { get; private set; }
        public LiteralRegex End { get; private set; }

        private ClassRegexElement() { }
        public ClassRegexElement(LiteralRegex start)
        {
            this.Start = start;
        }
        public ClassRegexElement(LiteralRegex start, LiteralRegex end)
        {
            this.Start = start;
            this.End = end;
        }

        public override string ToString()
        {
            return End != null ? $"{Start}-{End}" : Start.ToString();
        }
    }
}
