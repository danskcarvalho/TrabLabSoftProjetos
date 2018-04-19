using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Parsing
{
    public enum NodeType
    {
        Token,
        GrammarDefinition,
        CharsetDefinition,
        CharsetName,
        RegexClass,
        RegexClassElement,
        CharsetBody,
        CharsetExpression,
        CharsetExpressionElement,
        TerminalDefinition,
        AltRegexExpression,
        ConcatRegexExpression,
        SubRegexExpression,
        NonterminalDefinition,
        QuantifiedRegexExpression,
        NonterminalBody,
        NonterminalExpression,
        NonterminalProduction,
        ProductionName,
        OptionDefinition,
        OptionBody,
        NonterminalName
    }
}
