using Compilador.Common;
using Compilador.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Parsing
{
    public class ParserBase
    {
        private IReadOnlyList<Token> Tokens;
        private int CurrentToken = 0;
        private List<Node> NodeStack = new List<Node>();
        private List<Tuple<int, List<Token>>> Scopes = new List<Tuple<int, List<Token>>>();

        public ParserBase(IEnumerable<Token> tokens)
        {
            this.Tokens = tokens.ToList().AsReadOnly();
        }

        public Node Parse()
        {
            CurrentToken = 0;
            NodeStack.Clear();
            Scopes.Clear();

            ParseGrammarDefinition();
            return NodeStack[0];
        }

        #region [ Parsing ]
        void ParseGrammarDefinition()
        {
            while (!NextIs(TokenType.Eof))
            {
                if (NextIs(TokenType.OptionKeyword))
                    ParseOption();
                else if (NextIs(TokenType.Identifier))
                    ParseTerminalDefinition();
                else if (NextIs(TokenType.OCurlyOperator))
                    ParseCharsetDefinition();
                else if (NextIs(TokenType.LtOperator))
                    ParseNonterminalDefinition();
                else
                    throw GrammarException();
            }

            if (NodeStack.Count == 0)
                throw GrammarException("gramática vazia");

            Push(TokenType.Eof);
            Reduce(NodeType.GrammarDefinition);
        }

        #region [ Nonterminal ]
        private void ParseNonterminalDefinition()
        {
            Scope(() =>
            {
                ParseNonterminalName();
                Push(TokenType.EqOperator);
                ParseNonterminalBody();
                Push(TokenType.Eos);
                Reduce(NodeType.NonterminalDefinition, 4);
            });
        }

        private void ParseNonterminalBody()
        {
            ParseNonterminalExpression();
            Reduce(NodeType.NonterminalBody, 1);
        }

        private void ParseNonterminalExpression()
        {
            ParseNonterminalProduction();

            if (NextIs(TokenType.PipeOperator))
            {
                Push(TokenType.PipeOperator);
                ParseNonterminalExpression();
                Reduce(NodeType.NonterminalExpression, 3);
            }
            else
                Reduce(NodeType.NonterminalExpression, 1);
        }

        private void ParseNonterminalProduction()
        {
            var hasName = ParseProductionName();

            int numElements = 0;
            while (ParseProductionElement())
                numElements++;

            if (numElements == 0)
                throw GrammarException();

            Reduce(NodeType.NonterminalProduction, numElements + hasName);
        }

        private bool ParseProductionElement()
        {
            if (NextIs(TokenType.Identifier))
            {
                Push(TokenType.Identifier);
                return true;
            }
            else if (NextIs(TokenType.StringLiteral))
            {
                Push(TokenType.StringLiteral);
                return true;
            }
            else if (NextIs(TokenType.LtOperator))
            {
                ParseNonterminalName();
                return true;
            }
            else
                return false;
        }

        private int ParseProductionName()
        {
            if (NextIs(TokenType.AtOperator))
            {
                Push(TokenType.AtOperator);
                Push(TokenType.StringLiteral);
                Reduce(NodeType.ProductionName, 2);
                return 1;
            }
            else
                return 0;
        }

        private void ParseNonterminalName()
        {
            Push(TokenType.LtOperator);
            var madeIt = TryPush(TokenType.Identifier);
            Push(TokenType.GtOperator);
            Reduce(NodeType.NonterminalName, madeIt ? 3 : 2);
        }
        #endregion

        #region [ Charset ]
        private void ParseCharsetDefinition()
        {
            Scope(() =>
            {
                ParseCharsetName();
                Push(TokenType.EqOperator);
                ParseCharsetBody();
                Push(TokenType.Eos);
                Reduce(NodeType.CharsetDefinition, 4);
            });
        }

        private void ParseCharsetBody()
        {
            if (NextIs(TokenType.OSqrOperator))
            {
                ParseRegexClass();
                Reduce(NodeType.CharsetBody, 1);
            }
            else
            {
                ParseCharsetExpression();
                Reduce(NodeType.CharsetBody, 1);
            }
        }

        private void ParseCharsetExpression()
        {
            ParseCharsetExpressionElement();

            if(NextIs(TokenType.MinusOperator) || NextIs(TokenType.PlusOperator) || NextIs(TokenType.DivOperator))
            {
                PushOneOf(TokenType.MinusOperator, TokenType.PlusOperator, TokenType.DivOperator);
                ParseCharsetExpression();
                Reduce(NodeType.CharsetExpression, 3);
            }
            else
                Reduce(NodeType.CharsetExpression, 1);
        }

        private void ParseCharsetExpressionElement()
        {
            if (NextIs(TokenType.Identifier))
            {
                Push(TokenType.Identifier);
                Reduce(NodeType.CharsetExpressionElement, 1);
            }
            else if (NextIs(TokenType.OCurlyOperator))
            {
                Push(TokenType.OCurlyOperator);
                ParseCharsetExpression();
                Push(TokenType.ECurlyOperator);
                Reduce(NodeType.CharsetExpressionElement, 3);
            }
        }

        private void ParseCharsetName()
        {
            Push(TokenType.OCurlyOperator);
            Push(TokenType.Identifier);
            Push(TokenType.ECurlyOperator);
            Reduce(NodeType.CharsetName, 3);
        }
        #endregion

        #region [ Terminal Definitions ]
        private void ParseTerminalDefinition()
        {
            Scope(() =>
            {
                Push(TokenType.Identifier);
                Push(TokenType.EqOperator);
                ParseAltRegexExpression();
                Push(TokenType.Eos);
                Reduce(NodeType.TerminalDefinition, 4);
            });
        }

        private void ParseAltRegexExpression()
        {
            ParseConcatRegexExpression();
            if (NextIs(TokenType.PipeOperator))
            {
                Push(TokenType.PipeOperator);
                ParseAltRegexExpression();
                Reduce(NodeType.AltRegexExpression, 3);
            }
            else
                Reduce(NodeType.AltRegexExpression, 1);
        }

        private void ParseConcatRegexExpression()
        {
            int numElements = 0;
            while (ParseQuantifiedExpression())
                numElements++;

            if (numElements == 0)
                throw GrammarException();

            Reduce(NodeType.ConcatRegexExpression, numElements);
        }

        private bool ParseQuantifiedExpression()
        {
            if (!ParseSubRegexElement())
                return false;

            if(NextIs(TokenType.MultOperator) || NextIs(TokenType.PlusOperator) || NextIs(TokenType.QtOperator))
            {
                PushOneOf(TokenType.MultOperator, TokenType.PlusOperator, TokenType.QtOperator);
                Reduce(NodeType.QuantifiedRegexExpression, 2);
            }
            else
                Reduce(NodeType.QuantifiedRegexExpression, 1);

            return true;
        }

        private bool ParseSubRegexElement()
        {
            if (ParseBasicRegexElement())
            {
                Reduce(NodeType.SubRegexExpression, 1);
                return true;
            }
            else if (NextIs(TokenType.OSqrOperator))
            {
                ParseRegexClass();
                Reduce(NodeType.SubRegexExpression, 1);
                return true;
            }
            else if (NextIs(TokenType.AtOperator))
            {
                Push(TokenType.AtOperator);
                Push(TokenType.Identifier);
                Reduce(NodeType.SubRegexExpression, 2);
                return true;
            }
            else if (NextIs(TokenType.OCurlyOperator))
            {
                ParseCharsetName();
                Reduce(NodeType.SubRegexExpression, 1);
                return true;
            }
            else if (NextIs(TokenType.ORoundOperator))
            {
                Push(TokenType.ORoundOperator);
                ParseAltRegexExpression();
                Push(TokenType.ERoundOperator);
                Reduce(NodeType.SubRegexExpression, 3);
                return true;
            }
            else
                return false;
        }

        private void ParseRegexClass()
        {
            int numElements = 0;

            Push(TokenType.OSqrOperator);

            while (ParseClassElements())
                numElements++;

            if (numElements == 0)
                throw GrammarException();

            Push(TokenType.ESqrOperator);

            Reduce(NodeType.RegexClass, 2 + numElements);
        }

        private bool ParseClassElements()
        {
            if (!ParseBasicRegexElement())
                return false;

            if (NextIs(TokenType.MinusOperator))
            {
                Push(TokenType.MinusOperator);
                if (!ParseBasicRegexElement())
                    throw GrammarException();

                Reduce(NodeType.RegexClassElement, 3);
            }
            else
                Reduce(NodeType.RegexClassElement, 1);

            return true;
        }

        private bool ParseBasicRegexElement()
        {
            if (NextIs(TokenType.Identifier))
            {
                Push(TokenType.Identifier);
                return true;
            }
            else if (NextIs(TokenType.EscapeSequence))
            {
                Push(TokenType.EscapeSequence);
                return true;
            }
            else if (NextIs(TokenType.StringLiteral))
            {
                Push(TokenType.StringLiteral);
                return true;
            }
            else
                return false;
        }
        #endregion

        private void ParseOption()
        {
            Scope(() =>
            {
                Push(TokenType.OptionKeyword);
                Push(TokenType.Identifier);
                Push(TokenType.EqOperator);
                ParseOptionBody();
                Push(TokenType.Eos);
                Reduce(NodeType.OptionDefinition, 5);
            });
        }

        private void ParseOptionBody()
        {
            if (NextIs(TokenType.StringLiteral))
            {
                Push(TokenType.StringLiteral);
                if (NextIs(TokenType.StringLiteral))
                {
                    Push(TokenType.StringLiteral);
                    Reduce(NodeType.OptionBody, 2);
                }
                else
                    Reduce(NodeType.OptionBody, 1);
            }
            else if (NextIs(TokenType.Identifier))
            {
                Push(TokenType.Identifier);
                Reduce(NodeType.OptionBody, 1);
            }
            else
            {
                ParseNonterminalName();
                Reduce(NodeType.OptionBody, 1);
            }
        }
        #endregion

        #region [ Helpers ]
        Token Peek(int skip = 0)
        {
            if (CurrentScope() == null)
            {
                var index = CurrentToken + skip;
                if (index >= Tokens.Count)
                    return Tokens[Tokens.Count - 1];
                return Tokens[index];
            }
            else
            {
                var index = (CurrentToken + skip) - CurrentScope().Item1;
                if (index >= CurrentScope().Item2.Count)
                    return CurrentScope().Item2[CurrentScope().Item2.Count - 1];
                return CurrentScope().Item2[index];
            }
        }
        bool NextIs(TokenType type)
        {
            return Peek().Type == type;
        }
        Location CurrentLocation()
        {
            return Peek().Location;
        }
        Exception GrammarException(string message = null)
        {
            if (message == null)
            {
                if (Peek().Source != null)
                    message = $"sintaxe inválida próximo de {Peek().Source}";
                else
                    message = "sintaxe inválida";
            }
            return new GrammarException(CurrentLocation(), message);
        }

        void Push(TokenType type)
        {
            if (!NextIs(type))
                throw GrammarException();

            NodeStack.Add(Node.FromToken(Peek()));
            if (type != TokenType.Eos)
                CurrentToken++;
        }
        bool TryPush(TokenType type)
        {
            if (!NextIs(type))
                return false;

            NodeStack.Add(Node.FromToken(Peek()));
            CurrentToken++;
            return true;
        }
        void PushOneOf(params TokenType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (TryPush(types[i]))
                    return;
            }
            throw GrammarException();
        }

        void Reduce(NodeType type, int numElements = -1)
        {
            if(numElements <= 0)
            {
                var node = Node.FromNodes(type, NodeStack);
                NodeStack.Clear();
                NodeStack.Add(node);
            }
            else
            {
                var nodesToReduce = new List<Node>();
                for (int i = (NodeStack.Count - numElements); i < NodeStack.Count; i++)
                    nodesToReduce.Add(NodeStack[i]);
                var node = Node.FromNodes(type, nodesToReduce);
                for (int i = 0; i < numElements; i++)
                    NodeStack.RemoveAt(NodeStack.Count - 1);
                NodeStack.Add(node);
            }
        }
        void Scope(Action action)
        {
            var token = Peek();
            var scopedTokens = Tokens.Skip(CurrentToken + 1).TakeWhile(x => x.Location.Column > token.Location.Column).ToList();
            scopedTokens.Insert(0, Tokens.Skip(CurrentToken).First());

            if (scopedTokens[scopedTokens.Count - 1].Type == TokenType.Eof)
                scopedTokens.RemoveAt(scopedTokens.Count - 1);

            scopedTokens.Add(new Token(null, TokenType.Eos, Peek(scopedTokens.Count).Location));
            Scopes.Add(Tuple.Create(CurrentToken, scopedTokens));
            try
            {
                action();
            }
            finally
            {
                Scopes.RemoveAt(Scopes.Count - 1);
            }
        }
        Tuple<int, List<Token>> CurrentScope()
        {
            if (Scopes.Count == 0)
                return null;
            return Scopes[Scopes.Count - 1];
        }
        #endregion
    }
}
