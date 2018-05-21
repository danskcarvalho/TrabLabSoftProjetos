using System;
using Compilador.Common;

namespace Compilador.Grammar
{
    public class CharsetClassExpression : CharsetExpression
    {
        public ClassRegex ClassRegex { get; private set; } 
        public CharsetClassExpression(ClassRegex classRegex, Location location) : base(location)
        {
            this.ClassRegex = classRegex;
        }

        public override string ToString()
        {
            return ClassRegex.ToString();
        }
    }
}
