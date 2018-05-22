using Compilador.Common;
using Compilador.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Parsing
{
    class Node
    {
        public NodeType Type { get; private set; }
        public Location Location
        {
            get
            {
                if (Token != null)
                    return Token.Location;
                else
                    return Children[0].Location;
            }
        }
        public Token Token { get; private set; }
        public IReadOnlyList<Node> Children { get; private set; }

        private Node()
        {

        }

        public static Node FromToken(Token token)
        {
            var node = new Node();
            node.Type = NodeType.Token;
            node.Token = token;
            node.Children = new List<Node>();
            return node;
        }
        public static Node FromNodes(NodeType nodeType, IEnumerable<Node> children)
        {
            var node = new Node();
            node.Type = nodeType;
            node.Children = children.ToList().AsReadOnly();
            return node;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            ToString(builder, 0);
            return builder.ToString();
        }

        private void ToString(StringBuilder builder, int level)
        {
            if(Type == NodeType.Token)
            {
                AppendLevel(builder, level).AppendLine($"<{Token.ToString()} />");
                return;
            }

            AppendLevel(builder, level).AppendLine($"<{Type.ToString()}>");

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].ToString(builder, level + 1);
            }

            AppendLevel(builder, level).AppendLine($"</{Type.ToString()}>");
        }

        private StringBuilder AppendLevel(StringBuilder builder, int level)
        {
            for (int i = 0; i < level; i++)
            {
                builder.Append("    ");
            }
            return builder;
        }
    }
}
