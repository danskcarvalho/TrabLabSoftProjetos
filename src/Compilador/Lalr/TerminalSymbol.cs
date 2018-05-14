using System;
using System.Linq;

namespace Compilador.Lalr
{
    public class TerminalSymbol : Symbol, IEquatable<TerminalSymbol>, IComparable<TerminalSymbol>
    {
        public static readonly TerminalSymbol Eof = new TerminalSymbol("55d73a30-a1f9-4323-8a80-5f18efd9c840@Eof");

        public string Name { get; private set; }

        public TerminalSymbol(string name) {
            this.Name = name;
        }

        public bool Equals(TerminalSymbol other)
        {
            if (other == null)
                return false;

            return other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TerminalSymbol);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return IsIdentifier(Name) ? Name : (Name == Eof.Name ? "$eof" : $"'{Name}'");
        }

        private static bool IsIdentifier(string name) => char.IsLower(name[0]) && name.All(c => char.IsLetterOrDigit(c));

        public int CompareTo(TerminalSymbol other)
        {
            if (other == null) //other comes before
                return 1;
            if (GetType() != other.GetType())
                return string.Compare(this.GetType().FullName, other.GetType().FullName, StringComparison.Ordinal);

            return string.CompareOrdinal(this.Name, other.Name);
        }

        public override int CompareTo(object obj)
        {
            return CompareTo(obj as TerminalSymbol);
        }
    }
}
