using System;
using API.Semantics;
using Compilador.Grammar;

namespace SqlInterpreter
{
    public class SqlExecuter : Interpreter
    {
        private static GrammarDefinition _SqlGrammar;
        static SqlExecuter(){
            _SqlGrammar = GrammarDefinition.ReadFromResource<SqlExecuter>("SqlInterpreter.Grammar.BNFOutput");
        }
        public SqlExecuter()
        {
        }

        public override GrammarDefinition Grammar => _SqlGrammar;

        protected override void DefineAttributes()
        {
        }
    }
}
