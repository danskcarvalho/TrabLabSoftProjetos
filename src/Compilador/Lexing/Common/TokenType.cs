using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Lexing
{
    public enum TokenType
    {
        OptionKeyword,
        Whitespace,
        Comment,
        Identifier,
        StringLiteral,
        EscapeSequence,
        Eof,
        Eos,                // End Of Scope
        // OPERADORES
        DivOperator,        // /
        EqOperator,         // =
        PipeOperator,       // |
        LtOperator,         // <
        GtOperator,         // >
        OSqrOperator,       // [
        ESqrOperator,       // ]
        MinusOperator,      // -
        MultOperator,       // *
        PlusOperator,       // +
        ORoundOperator,     // (
        ERoundOperator,     // )
        QtOperator,         // ?
        OCurlyOperator,     // {
        ECurlyOperator,     // }
        AtOperator          // @
        /////////////
    }
}
