using API.Lexing;
using Compilador.Common;
using Compilador.Lalr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Parsing
{
    public abstract class Node
    {
        public Location Location { get; private set; }
        public abstract string Name { get; }
        public Node Parent { get; private set; }
        public abstract Token Token { get; }
        public abstract IReadOnlyList<Node> Children { get; }

        public Node(Location location)
        {
            this.Location = location;
        }

        public static Node FromToken(Token token)
        {
            return new TokenNode(token);
        }
        public static Node FromNodes(string name, GrammarProduction production, IEnumerable<Node> children)
        {
            return new ReducedNode(name, production, children);
        }
    }

    public class TokenNode : Node
    {
        private static IReadOnlyList<Node> EmptyChildren = new List<Node>().AsReadOnly();
        private readonly Token _token;
        public override Token Token { get => _token; }
        public override IReadOnlyList<Node> Children => EmptyChildren;
        public override string Name => Token.Name;

        public TokenNode(Token token) : base(token.Location)
        {
            _token = token;
        }

        public override string ToString()
        {
            return $"<{Token.ToString()} />";
        }
    }

    public class ReducedNode : Node
    {
        public GrammarProduction Production { get; private set; }
        public override IReadOnlyList<Node> Children { get => _children; }
        public override Token Token => null;
        private string _Name;
        private IReadOnlyList<Node> _children;

        public override string Name => _Name;

        public ReducedNode(string name, GrammarProduction production, IEnumerable<Node> children) : base(children.First().Location)
        {
            this._Name = name;
            Production = production;
            _children = children.ToList().AsReadOnly();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            ToString(this, builder, 0);
            return builder.ToString();
        }

        private static void ToString(TokenNode tkNode, StringBuilder builder, int level)
        {
            AppendLevel(builder, level).AppendLine($"<{tkNode.Token.ToString()} />");
        }

        private static void ToString(ReducedNode rdNode, StringBuilder builder, int level)
        {
            AppendLevel(builder, level).AppendLine($"<{rdNode.Production.Head.Name}>");

            for (int i = 0; i < rdNode.Children.Count; i++)
            {
                if (rdNode.Children[i] is ReducedNode)
                    ToString(rdNode.Children[i] as ReducedNode, builder, level + 1);
                else
                    ToString(rdNode.Children[i] as TokenNode, builder, level + 1);
            }

            AppendLevel(builder, level).AppendLine($"</{rdNode.Production.Head.Name}>");
        }

        private static StringBuilder AppendLevel(StringBuilder builder, int level)
        {
            for (int i = 0; i < level; i++)
            {
                builder.Append("    ");
            }
            return builder;
        }
    }
}
