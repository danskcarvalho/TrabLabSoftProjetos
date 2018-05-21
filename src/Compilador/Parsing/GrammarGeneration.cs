using Compilador.Common;
using Compilador.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilador.Lalr;

namespace Compilador.Parsing
{
    public static class GrammarGeneration
    {
        public static GrammarDefinition Build(Node node)
        {
            NodeVisitor visitor = new NodeVisitor(node);

            var options = OnOptionDefinition(visitor);
            OnCharsetDefinition(visitor);
            OnRegexDefinition(visitor);
            OnProductionDefinition(visitor);
            return OnGrammarDefinition(visitor, options);
        }

        private static void OnProductionDefinition(NodeVisitor visitor){
            visitor.On(NodeType.NonterminalProduction, n =>
            {
                var elements = new List<Symbol>();
                var hasName = n.Children.Count >= 1 && n.Children[0].Type == NodeType.ProductionName;
                var prod = hasName ? n.Children.Skip(1) : n.Children;
                foreach (var item in prod)
                {
                    if (item.Type == NodeType.Token && item.Token.Type == Lexing.TokenType.Identifier)
                        elements.Add(new TerminalSymbol(item.Token.Source));
                    else if (item.Type == NodeType.Token && item.Token.Type == Lexing.TokenType.StringLiteral)
                        elements.Add(new TerminalSymbol(item.Token.Value));
                    else if (item.Children.Count == 3)
                        elements.Add(new NonterminalSymbol(item.Children[1].Token.Source));
                }

                string name = hasName ? n.Children[0].Children[1].Token.Source : Guid.NewGuid().ToString();
                visitor.Emit(Tuple.Create(name, elements, n.Location));
            });
            visitor.On(NodeType.NonterminalDefinition, n =>
            {
                var head = n.Children[0].Children[1].Token.Source;
                List<Tuple<string, GrammarProduction, Location>> productions = new List<Tuple<string, GrammarProduction, Location>>();
                visitor.OnEmit<Tuple<string, List<Symbol>, Location>>(p =>
                {
                    productions.Add(Tuple.Create(p.Item1, new GrammarProduction(new NonterminalSymbol(head), new SymbolString(p.Item2)), p.Item3));
                });
                visitor.OnFinished(() => {
                    foreach (var p in productions)
                    {
                        visitor.Emit(p.Item2);
                    }
                    var names = new HashSet<string>();
                    foreach (var p in productions)
                    {
                        if (names.Contains(p.Item1))
                            throw new GrammarException(p.Item3, $"nome repetido: {p.Item1}");
                    }
                    visitor.Emit(productions);
                });
            });
        }
        private static void OnRegexDefinition(NodeVisitor visitor){
            visitor.On(NodeType.SubRegexExpression, (n) => {
                var r = GetTerminalRegex(n);
                if (r != null)
                    visitor.Emit(r);
            });
            visitor.On(NodeType.RegexClassElement, n =>
            {
                if (n.Children.Count == 1)
                    visitor.Emit(new ClassRegexElement(GetTerminalRegexFromToken(n.Children[0])));
                else
                    visitor.Emit(new ClassRegexElement(GetTerminalRegexFromToken(n.Children[0]), GetTerminalRegexFromToken(n.Children[2])));
            });
            visitor.On(NodeType.RegexClass, n => {
                List<ClassRegexElement> elements = new List<ClassRegexElement>();
                visitor.OnEmit<ClassRegexElement>(e => {
                    elements.Add(e);
                });
                visitor.OnFinished(() => {
                    visitor.Emit(new ClassRegex(elements, n.Location));
                });
            });
            visitor.On(NodeType.QuantifiedRegexExpression, n =>
            {
                Regex rg = null;
                visitor.OnEmit<Regex>(r => {
                    rg = r;
                });

                visitor.OnFinished(() => {
                    if (n.Children.Count == 2 && n.Children[1].Token.Type == Lexing.TokenType.PlusOperator)
                        visitor.Emit(new QuantifiedRegex(QuantificationType.OneOrMore, rg, n.Location));
                    else if (n.Children.Count == 2 && n.Children[1].Token.Type == Lexing.TokenType.MultOperator)
                        visitor.Emit(new QuantifiedRegex(QuantificationType.ZeroOrMore, rg, n.Location));
                    else if (n.Children.Count == 2 && n.Children[1].Token.Type == Lexing.TokenType.QtOperator)
                        visitor.Emit(new QuantifiedRegex(QuantificationType.ZeroOrOne, rg, n.Location));
                    else
                        visitor.Emit(rg);
                });
            });
            visitor.On(NodeType.ConcatRegexExpression, n =>
            {
                List<Regex> rgs = new List<Regex>();
                visitor.OnEmit<Regex>(r => {
                    rgs.Add(r);
                });

                visitor.OnFinished(() =>
                {
                    if (rgs.Count == 1)
                        visitor.Emit(rgs[0]);
                    else
                        visitor.Emit(new SequentialRegex(rgs, n.Location));
                });
            });
            visitor.On(NodeType.AltRegexExpression, n =>
            {
                List<Regex> rgs = new List<Regex>();
                visitor.OnEmit<Regex>(r => {
                    if (r is AlternativeRegex)
                    {
                        foreach (var item in ((AlternativeRegex)r).Alternatives)
                        {
                            rgs.Add(item);
                        }
                    }
                    else
                        rgs.Add(r);
                });

                visitor.OnFinished(() =>
                {
                    if (rgs.Count == 1)
                        visitor.Emit(rgs[0]);
                    else
                        visitor.Emit(new AlternativeRegex(rgs, n.Location));
                });
            });
            visitor.On(NodeType.TerminalDefinition, n =>
            {
                Regex rg = null;
                visitor.OnEmit<Regex>(r => {
                    rg = r;
                });

                visitor.OnFinished(() =>
                {
                    visitor.Emit(new RegexDefinition(n.Children[0].Token.Source, rg, n.Location));
                });
            });
        }

        private static Regex GetTerminalRegex(Node n){
            if (n.Children[0].Type == NodeType.Token && n.Children[0].Token.Type == Lexing.TokenType.Identifier)
            {
                return new LiteralRegex(n.Children[0].Token.Source, n.Children[0].Token.Location);
            }
            else if (n.Children[0].Type == NodeType.Token && n.Children[0].Token.Type == Lexing.TokenType.EscapeSequence)
            {
                return new LiteralRegex(n.Children[0].Token.Value, n.Children[0].Token.Location);
            }
            else if (n.Children[0].Type == NodeType.Token && n.Children[0].Token.Type == Lexing.TokenType.StringLiteral)
            {
                return new LiteralRegex(n.Children[0].Token.Value, n.Children[0].Token.Location);
            }
            else if (n.Children[0].Type == NodeType.Token && n.Children[0].Token.Type == Lexing.TokenType.AtOperator)
            {
                return new ReferenceRegex(n.Children[1].Token.Source, n.Children[0].Token.Location);
            }
            else if (n.Children[0].Type == NodeType.CharsetName)
            {
                return new CharsetRegex(n.Children[0].Children[1].Token.Source, n.Location);
            }
            else
                return null;
        }
        private static Regex GetTerminalRegexFromToken(Node n)
        {
            if (n.Type == NodeType.Token && n.Token.Type == Lexing.TokenType.Identifier)
            {
                return new LiteralRegex(n.Token.Source, n.Token.Location);
            }
            else if (n.Type == NodeType.Token && n.Token.Type == Lexing.TokenType.EscapeSequence)
            {
                return new LiteralRegex(n.Token.Value, n.Token.Location);
            }
            else if (n.Type == NodeType.Token && n.Token.Type == Lexing.TokenType.StringLiteral)
            {
                return new LiteralRegex(n.Token.Value, n.Token.Location);
            }
            else if (n.Type == NodeType.Token && n.Token.Type == Lexing.TokenType.AtOperator)
            {
                return new ReferenceRegex(n.Children[1].Token.Source, n.Token.Location);
            }
            else if (n.Type == NodeType.CharsetName)
            {
                return new CharsetRegex(n.Children[1].Token.Source, n.Location);
            }
            else
                return null;
        }

        private static void OnCharsetDefinition(NodeVisitor visitor)
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
            visitor.On(NodeType.CharsetDefinition, n =>
            {
                string name = n.Children[0].Children[1].Token.Source;
                CharsetExpression expr = null;
                visitor.OnEmit<CharsetExpression>(m =>
                {
                    expr = m;
                });
                visitor.OnEmit<ClassRegex>(m => {
                    expr = new CharsetClassExpression(m, m.Location);
                });
                visitor.OnFinished(() =>
                {
                    visitor.Emit(new CharsetDefinition(name, expr, n.Location));
                });
            });
        }

        private static GrammarDefinition OnGrammarDefinition(NodeVisitor visitor, Options options)
        {
            GrammarDefinition definition = null;
            List<RegexDefinition> regexDefinitions = new List<RegexDefinition>();
            List<CharsetDefinition> charsetDefinitions = new List<CharsetDefinition>();
            List<GrammarProduction> grammarProductions = new List<GrammarProduction>();
            Dictionary<GrammarProduction, string> productionNames = new Dictionary<GrammarProduction, string>();

            visitor.On(NodeType.GrammarDefinition, n =>
            {
                visitor.OnEmit((RegexDefinition o) => {
                    regexDefinitions.Add(o);
                });
                visitor.OnEmit((CharsetDefinition o) => {
                    charsetDefinitions.Add(o);
                });
                visitor.OnEmit((GrammarProduction o) => {
                    grammarProductions.Add(o);
                });
                visitor.OnEmit((List<Tuple<string, GrammarProduction, Location>> o) => {
                    foreach (var item in o)
                    {
                        if (productionNames.Any(x => x.Value == item.Item1))
                            throw new GrammarException(item.Item3, $"regra de produção com nome repetido {item.Item1}");
                        productionNames.Add(item.Item2, item.Item1);
                    }
                });
                visitor.OnFinished(() =>
                {
                    if (options.StartSymbol == null)
                        throw new GrammarException(new Location(), "sem símbolo inicial");
                    definition = new GrammarDefinition(
                        options.CaseInsensitive,
                        options.LineComment,
                        options.StartBlockComment,
                        options.EndBlockComment,
                        regexDefinitions,
                        charsetDefinitions,
                        productionNames,
                        grammarProductions,
                        new NonterminalSymbol(options.StartSymbol));
                });
            });
            visitor.Visit();
            return definition;
        }

        private static Options OnOptionDefinition(NodeVisitor visitor)
        {
            var options = new Options();

            visitor.On(NodeType.OptionDefinition, (n) =>
            {
                if (n.Children[1].Token.Source == "CaseInsensitive")
                {
                    if (n.Children[3].Children.Count != 1)
                        throw new GrammarException(n.Children[3].Location, $"esperado yes ou no para CaseInsensitive");

                    options.CaseInsensitive = n.Children[3].Children[0].Token.Source.ToLowerInvariant() == "yes";
                    if (!options.CaseInsensitive && n.Children[3].Children[0].Token.Source.ToLowerInvariant() != "no")
                        throw new GrammarException(n.Children[3].Location, $"esperado yes ou no para CaseInsensitive");

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
            public bool CaseInsensitive { get; set; }
            public string StartSymbol { get; set; }
            public string LineComment { get; set; }
            public string StartBlockComment { get; set; }
            public string EndBlockComment { get; set; }
        }
    }
}
