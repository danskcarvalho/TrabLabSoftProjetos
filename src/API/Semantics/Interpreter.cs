using API.Parsing;
using Compilador.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Semantics
{
    public abstract class Interpreter
    {
        public Interpreter()
        {
            DefineAttributes();
        }

        public abstract GrammarDefinition Grammar { get; }

        protected abstract void DefineAttributes();
        protected Action<Node> Interpretation { get; set; }

        public void Execute(string source)
        {
            var node = LalrParser.Parse(Grammar, source);
            Interpretation(node);
        }

        protected SynthesizedAttribute<T> Synthesized<T>(Action<SynthesizedAttributeContext<T>> action,
            Func<T, T, T> aggregationFunction = null)
        {
            return new SynthesizedAttribute<T>(action, aggregationFunction);
        }

        protected InheritedAttribute<T> Inherited<T>(Action<SynthesizedAttributeContext<T>> action)
        {
            return new InheritedAttribute<T>(action);
        }
    }
}
