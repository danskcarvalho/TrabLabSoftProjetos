using API.Lexing;
using Compilador.Common;
using Compilador.Grammar;
using Compilador.Lalr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Parsing
{
    public static class LalrParser
    {
        public static Node Parse(GrammarDefinition grammar, string source)
        {
            FullLalrState lalrState = new FullLalrState();
            lalrState.StateStack.Add(grammar.Table[0]); //push state 0
            foreach (var tk in Lexer.Lex(grammar, source))
            {
                var currentSymbol = new TerminalSymbol(tk.Name);
                DontConsumeToken:
                {
                    var currentState = lalrState.StateStack[lalrState.StateStack.Count - 1];

                    if (!currentState.ContainsKey(currentSymbol))
                        throw new GrammarException(tk.Location, $"token inesperado {tk.Source}");

                    var action = currentState[currentSymbol].First();
                    if (action is LalrShift)
                    {
                        var s = action as LalrShift;
                        lalrState.NodeStack.Add(Node.FromToken(tk));
                        lalrState.StateStack.Add(s.State);
                    }
                    else if (action is LalrReduce)
                    {
                        var prod = ((LalrReduce)action).Production;
                        Reduce(prod, lalrState);
                        currentState = lalrState.StateStack[lalrState.StateStack.Count - 1];
                        lalrState.StateStack.Add(((LalrGoto)currentState[prod.Head].First()).State);
                        goto DontConsumeToken;
                    }
                    else
                    {
                        //accept
                        return lalrState.NodeStack.First();
                    }
                }
            }

            //should never get here
            throw new InvalidOperationException();
        }

        private static void Reduce(GrammarProduction production, FullLalrState lalrState)
        {
            List<Node> toBeReduced = new List<Node>();
            for (int i = 0; i < production.Body.Count; i++)
            {
                toBeReduced.Add(lalrState.NodeStack[lalrState.NodeStack.Count - 1]);
                lalrState.NodeStack.RemoveAt(lalrState.NodeStack.Count - 1);
                lalrState.StateStack.RemoveAt(lalrState.StateStack.Count - 1);
            }

            toBeReduced.Reverse();
            lalrState.NodeStack.Add(Node.FromNodes(production, toBeReduced));
        }

        private class FullLalrState
        {
            public List<LalrState> StateStack = new List<LalrState>();
            public List<Node> NodeStack = new List<Node>();
        }
    }
}
