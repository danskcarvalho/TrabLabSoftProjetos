using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Lexing
{
    public class Token
    {
        public TokenType Type { get; private set; }
        public string Value { get; private set; }
        public Location Location { get; private set; }
        public string Source { get; private set; }

        public Token(string source, TokenType tokenType, Location location)
        {
            this.Type = tokenType;
            this.Location = location;
            this.Source = source;
        }
        public Token(string source, TokenType tokenType, string value, Location location)
        {
            this.Type = tokenType;
            this.Value = value;
            this.Location = location;
            this.Source = source;
        }

        public override string ToString()
        {
            if (Type == TokenType.Identifier)
                return "ID " + Value;
            else
                return Type.ToString();
        }
    }
}
