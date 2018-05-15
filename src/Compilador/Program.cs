using Compilador.Common;
using Compilador.Driving;
using Compilador.Lalr;
using Compilador.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Compilador.Common.Memoize;

namespace Compilador
{
    class Program
    {
        static void Main(string[] args)
        {
            var productions = new GrammarProduction[]
            {
                GrammarProduction.Create("Goal", "<Sums>"),
                GrammarProduction.Create("Sums", "<Sums> + <Products>"),
                GrammarProduction.Create("Sums", "<Products>"),
                GrammarProduction.Create("Products", "<Products> * <Value>"),
                GrammarProduction.Create("Products", "<Value>"),
                GrammarProduction.Create("Value", "Int"),
                GrammarProduction.Create("Value", "Id")
            };
            GrammarProductionDatabase db = new GrammarProductionDatabase(productions, new NonterminalSymbol("Goal"));
            var table = LalrContext.ComputeTable(db);
        }
    }
}
