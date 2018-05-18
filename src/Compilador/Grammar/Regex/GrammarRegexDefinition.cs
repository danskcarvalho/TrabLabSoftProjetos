using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public class GrammarRegexDefinition
    {
        public string DefinitionName { get; set; }
        public GrammarRegex Regex { get; set; }
        public Location Location { get; set; }

        public GrammarRegexDefinition(string definitionName, GrammarRegex regex, Location location)
        {
            this.DefinitionName = definitionName;
            this.Regex = regex;
            this.Location = location;
        }
    }
}
