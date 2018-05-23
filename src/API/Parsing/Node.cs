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

        public Node(Location location)
        {
            this.Location = location;
        }

        public static Node FromToken(Token token)
        {
            return new TokenNode(token);
        }
        public static Node FromNodes(GrammarProduction production, IEnumerable<Node> children)
        {
            return new ReducedNode(production, children);
        }
    }

    public class TokenNode : Node
    {
        public Token Token { get; private set; }

        public TokenNode(Token token) : base(token.Location)
        {
            Token = token;
        }

        public override string ToString()
        {
            return $"<{Token.ToString()} />";
        }
    }

    public class ReducedNode : Node
    {
        public GrammarProduction Production { get; private set; }
        public IReadOnlyList<Node> Children { get; private set; }

        public ReducedNode(GrammarProduction production, IEnumerable<Node> children) : base(children.First().Location)
        {
            Production = production;
            Children = children.ToList().AsReadOnly();
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
