using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public enum QuantificationType
    {
        ZeroOrMore,
        OneOrMore,
        ZeroOrOne
    }
    [Serializable]
    public class QuantifiedRegex : Regex
    {
        public QuantificationType Type { get; private set; }
        public Regex Quantified { get; private set; }

        public QuantifiedRegex(QuantificationType type, Regex quantified, Location location) : base(location)
        {
            this.Type = type;
            this.Quantified = quantified;
        }
        private QuantifiedRegex()
        {

        }

        public override IEnumerable<Regex> Children => new List<Regex> { Quantified };

        public override string ToString()
        {
            switch (Type)
            {
                case QuantificationType.ZeroOrMore:
                    return $"({Quantified})*";
                case QuantificationType.OneOrMore:
                    return $"({Quantified})+";
                case QuantificationType.ZeroOrOne:
                    return $"({Quantified})?";
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
