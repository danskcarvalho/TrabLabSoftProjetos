using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Lexing
{
    public class Token
    {
        public const string EofName = "55d73a30-a1f9-4323-8a80-5f18efd9c840@Eof";
        public const string WhitespaceName = "55d73a30-a1f9-4323-8a80-5f18efd9c840@Whitespace";

        public string Name { get; private set; }
        public string Source { get; private set; }
        public Location Location { get; private set; }

        public Token(string name, string source, Location location)
        {
            Name = name;
            Source = source;
            Location = location;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
