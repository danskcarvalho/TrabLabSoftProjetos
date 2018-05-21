using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class CharsetNameExpression : CharsetExpression
    {
        public string Name { get; set; }

        public CharsetNameExpression(string name, Location location) : base(location)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
