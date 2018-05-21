﻿using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public enum QuantificationType
    {
        ZeroOrMore,
        OneOrMore,
        ZeroOrOne
    }
    public class QuantifiedRegex : Regex
    {
        public QuantificationType Type { get; private set; }
        public Regex Quantified { get; private set; }

        public QuantifiedRegex(QuantificationType type, Regex quantified, Location location) : base(location)
        {
            this.Type = type;
            this.Quantified = quantified;
        }
    }
}