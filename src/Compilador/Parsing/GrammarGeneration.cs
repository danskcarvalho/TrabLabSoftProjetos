using Compilador.Common;
using Compilador.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Parsing
{
    public static class GrammarGeneration
    {
        public static GrammarDefinition Build(Node node)
        {
            NodeVisitor visitor = new NodeVisitor(node);

            var options = OnOptionDefinition(visitor);
            var charsets = OnCharsetDefinition(visitor);

            return OnGrammarDefinition(visitor);
        }

        private static List<CharsetDefinition> OnCharsetDefinition(NodeVisitor visitor)
        {
            visitor.On(NodeType.CharsetExpressionElement, n =>
            {
                if (n.Children.Count == 1)
                    visitor.Emit(new CharsetNameExpression(n.Children[0].Token.Source, n.Children[0].Location));
            });
            visitor.On(NodeType.CharsetExpression, n =>
            {
                CharsetExpression left = null, right = null;
                visitor.OnEmit<CharsetExpression>(expr =>
                {
                    if (left == null)
                        left = expr;
                    else if (right == null)
                        right = expr;
                });
                visitor.OnFinished(() =>
                {
                    if (n.Children.Count == 3)
                    {
                        if (n.Children[1].Token.Type == Lexing.TokenType.PlusOperator)
                            visitor.Emit(new CharsetBinaryExpression(CharsetBinaryOperator.Plus, left, right, n.Children[0].Location));
                        else if (n.Children[1].Token.Type == Lexing.TokenType.MinusOperator)
                            visitor.Emit(new CharsetBinaryExpression(CharsetBinaryOperator.Minus, left, right, n.Children[0].Location));
                        else
                            visitor.Emit(new CharsetBinaryExpression(CharsetBinaryOperator.Div, left, right, n.Children[0].Location));
                    }
                    else
                        visitor.Emit(left);
                });
            });
            var charsets = new List<CharsetDefinition>();
            visitor.On(NodeType.CharsetDefinition, n =>
            {
                string name = n.Children[0].Children[1].Token.Source;
                CharsetExpression expr = null;
                visitor.OnEmit<CharsetExpression>(m =>
                {
                    expr = m;
                });
                visitor.OnFinished(() =>
                {
                    charsets.Add(new CharsetDefinition(name, expr, n.Location));
                });
            });
            return charsets;
        }

        private static GrammarDefinition OnGrammarDefinition(NodeVisitor visitor)
        {
            GrammarDefinition definition = null;
            visitor.OnEmit<GrammarDefinition>(o =>
            {
                definition = o;
            });
            visitor.Visit();
            return definition;
        }

        private static Options OnOptionDefinition(NodeVisitor visitor)
        {
            var options = new Options();

            visitor.On(NodeType.OptionDefinition, (n) =>
            {
                if (n.Children[1].Token.Source == "CaseSensitive")
                {
                    if (n.Children[3].Children.Count != 1)
                        throw new GrammarException(n.Children[3].Location, $"esperado yes ou no para CaseSensitive");

                    options.CaseSensitive = n.Children[3].Children[0].Token.Source.ToLowerInvariant() == "yes";
                    if (!options.CaseSensitive && n.Children[3].Children[0].Token.Source.ToLowerInvariant() != "no")
                        throw new GrammarException(n.Children[3].Location, $"esperado yes ou no para CaseSensitive");

                }
                else if (n.Children[1].Token.Source == "StartSymbol")
                {
                    if (n.Children[3].Children.Count != 1)
                        throw new GrammarException(n.Children[3].Location, $"esperado 1 não terminal para StartSymbol");

                    if (n.Children[3].Children[0].Type != NodeType.NonterminalName)
                        throw new GrammarException(n.Children[3].Location, $"esperado 1 não terminal para StartSymbol");

                    if (n.Children[3].Children[0].Children.Count != 3)
                        throw new GrammarException(n.Children[3].Location, $"esperado 1 não terminal não vazio");

                    options.StartSymbol = n.Children[3].Children[0].Children[1].Token.Source;
                }
                else if (n.Children[1].Token.Source == "LineComment")
                {
                    if (n.Children[3].Children.Count != 1)
                        throw new GrammarException(n.Children[3].Location, $"esperado 1 literal para LineComment");

                    if (n.Children[3].Children[0].Token.Type != Lexing.TokenType.StringLiteral)
                        throw new GrammarException(n.Children[3].Location, $"esperado 1 literal para LineComment");

                    options.LineComment = n.Children[3].Children[0].Token.Value ?? n.Children[3].Children[0].Token.Source;
                }
                else if (n.Children[1].Token.Source == "BlockComment")
                {
                    if (n.Children[3].Children.Count != 2)
                        throw new GrammarException(n.Children[3].Location, $"esperado 2 literais para BlockComment");
                    if (n.Children[3].Children[0].Token.Type != Lexing.TokenType.StringLiteral ||
                        n.Children[3].Children[1].Token.Type != Lexing.TokenType.StringLiteral)
                        throw new GrammarException(n.Children[3].Location, $"esperado 2 literais para BlockComment");

                    options.StartBlockComment = n.Children[3].Children[0].Token.Value ?? n.Children[3].Children[0].Token.Source;
                    options.EndBlockComment = n.Children[3].Children[1].Token.Value ?? n.Children[3].Children[1].Token.Source;
                }
                else
                    throw new GrammarException(n.Children[1].Location, $"token inesperado {n.Children[1].Token.Source}");
            });

            return options;
        }

        private class Options
        {
            public bool CaseSensitive { get; set; }
            public string StartSymbol { get; set; }
            public string LineComment { get; set; }
            public string StartBlockComment { get; set; }
            public string EndBlockComment { get; set; }
        }
    }
}
