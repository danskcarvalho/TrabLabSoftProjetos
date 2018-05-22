using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class RegexDefinition
    {
        public string Name { get; set; }
        public Regex Regex { get; set; }
        public Location Location { get; set; }

        public RegexDefinition(string definitionName, Regex regex, Location location)
        {
            this.Name = definitionName;
            this.Regex = regex;
            this.Location = location;
        }
        private RegexDefinition()
        {

        }

        public override string ToString()
        {
            return $"{Name} = {Regex}";
        }
    }
}
