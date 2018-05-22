using System;
using Compilador.Common;

namespace Compilador.Grammar
{
    [Serializable]
    public class CharsetClassExpression : CharsetExpression
    {
        public ClassRegex ClassRegex { get; private set; } 
        public CharsetClassExpression(ClassRegex classRegex, Location location) : base(location)
        {
            this.ClassRegex = classRegex;
        }
        private CharsetClassExpression() { }

        public override string ToString()
        {
            return ClassRegex.ToString();
        }

        public override bool Contains(GrammarDefinition grammar, char c)
        {
            return ClassRegex.Contains(grammar, c);
        }
    }
}
