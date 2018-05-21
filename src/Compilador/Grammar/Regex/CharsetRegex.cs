using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class CharsetRegex : Regex
    {
        public string CharsetName { get; private set; }
        public CharsetRegex(string charsetName, Location location) : base(location)
        {
            this.CharsetName = charsetName;
        }
        private CharsetRegex()
        {

        }

        public override string ToString()
        {
            return "{" + CharsetName + "}";
        }

        public override IEnumerable<Regex> Children => new List<Regex>();
    }
}
