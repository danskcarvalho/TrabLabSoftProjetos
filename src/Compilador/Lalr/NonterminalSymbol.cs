using System;
namespace Compilador.Lalr
{
    public class NonterminalSymbol : Symbol, IEquatable<NonterminalSymbol>, IComparable<NonterminalSymbol>
    {
        public static readonly NonterminalSymbol StartingSymbol = new NonterminalSymbol("26574734-170f-4e59-bb1e-121745370006@StartingSymbol");
        public string Name { get; private set; }

        public NonterminalSymbol(string name)
        {
            this.Name = name;
        }

        public bool Equals(NonterminalSymbol other)
        {
            if (other == null)
                return false;

            return other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NonterminalSymbol);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return $"<{(Name == StartingSymbol.Name ? "$start" : Name)}>";
        }

        public int CompareTo(NonterminalSymbol other)
        {
            if (other == null) //other comes before
                return 1;
            if (GetType() != other.GetType())
                return string.Compare(this.GetType().FullName, other.GetType().FullName, StringComparison.Ordinal);

            return string.CompareOrdinal(this.Name, other.Name);
        }

        public override int CompareTo(object obj)
        {
            return CompareTo(obj as NonterminalSymbol);
        }
    }
}
