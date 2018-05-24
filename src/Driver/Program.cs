using API.Semantics;
using Compilador.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            var grammar = Compilador.Driving.Driver.Run(args);
            var interpreter = new Calculator(grammar);
            while (true)
            {
                var s = Console.ReadLine();
                if (s.ToLowerInvariant() == "exit")
                    return;
                interpreter.Execute(s);
            }
        }
    }

    class Calculator : Interpreter
    {
        private GrammarDefinition _Grammar;
        public Calculator(GrammarDefinition grammar)
        {
            this._Grammar = grammar;
        }

        public override GrammarDefinition Grammar => _Grammar;

        protected override void DefineAttributes()
        {
            SynthesizedAttribute<int> Value = null;
            Value = Synthesized<int>(ctx =>
            {
                ctx.On("Integer", n => int.Parse(n.Token.Source));
                ctx.On("Negate", n => Value[n.Children[1]]);
                ctx.On("Div", n => Value[n.Children[0]] / Value[n.Children[2]]);
                ctx.On("Mult", n => Value[n.Children[0]] * Value[n.Children[2]]);
                ctx.On("Sub", n => Value[n.Children[0]] - Value[n.Children[2]]);
                ctx.On("Plus", n => Value[n.Children[0]] + Value[n.Children[2]]);
            });
            Interpretation = (n) => Console.WriteLine(Value[n]);
        }
    }
}
