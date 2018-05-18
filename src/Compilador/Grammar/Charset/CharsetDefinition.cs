using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class CharsetDefinition
    {
        public string Name { get; private set; }
        public CharsetExpression Expression { get; private set; }
        public Location Location { get; set; }

        public CharsetDefinition(string name, CharsetExpression expression, Location location)
        {
            Name = name;
            Expression = expression;
            Location = location;
        }
    }
}
