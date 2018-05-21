using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class ReferenceRegex : Regex
    {
        public ReferenceRegex(string reference, Location location) : base(location)
        {
            Reference = reference;
        }
        private ReferenceRegex()
        {

        }

        public override IEnumerable<Regex> Children => new List<Regex>();

        public string Reference { get; set; }

        public override string ToString()
        {
            return "@" + Reference;
        }
    }
}
