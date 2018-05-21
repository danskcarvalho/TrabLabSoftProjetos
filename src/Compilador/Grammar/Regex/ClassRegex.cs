using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class ClassRegex : Regex
    {
        public IReadOnlyList<ClassRegexElement> Elements { get; private set; }
        public ClassRegex(IEnumerable<ClassRegexElement> elements, Location location) : base(location)
        {
            this.Elements = elements.ToList().AsReadOnly();
        }

        public override string ToString()
        {
            return "[" + string.Join(" ", Elements) + "]";
        }
    }

    public class ClassRegexElement
    {
        public Regex Start { get; private set; }
        public Regex End { get; private set; }

        public ClassRegexElement(Regex start)
        {
            this.Start = start;
        }
        public ClassRegexElement(Regex start, Regex end)
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
