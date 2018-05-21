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
    }
    [Serializable]
    public class ClassRegexElement
    {
        public Regex Start { get; private set; }
        public Regex End { get; private set; }

        private ClassRegexElement() { }
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
