using System;
namespace Compilador.Lalr
{
    public class GrammarProduction : IEquatable<GrammarProduction>, IComparable<GrammarProduction>, IComparable
    {
        public NonterminalSymbol Head { get; private set; }
        public SymbolString Body { get; private set; }

        public GrammarProduction(NonterminalSymbol head, SymbolString body){
            this.Head = head;
            this.Body = body;
        }

        public bool Equals(GrammarProduction other)
        {
            if (other == null)
                return false;
            if (object.ReferenceEquals(other, this))
                return true;
            
            return Head == other.Head && Body == other.Body;
        }

        public override bool Equals(object obj) => Equals(obj as GrammarProduction);

        private int? hashcode = null;
        public override int GetHashCode()
        {
            if (hashcode.HasValue)
                return hashcode.Value;
            
            hashcode = Head.GetHashCode() * 31 + Body.GetHashCode();
            return hashcode.Value;
        }
        public override string ToString() => $"{Head.ToString()} = {Body.ToString()}";

        public int CompareTo(GrammarProduction other)
        {
            if (other == null)
                return 1;
            var cp = this.Head.CompareTo(other);
            if (cp != 0)
                return cp;
            return this.Body.CompareTo(other);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as GrammarProduction);
        }

        public static bool operator ==(GrammarProduction a, GrammarProduction b)
        {
            if (object.ReferenceEquals(a, b))
                return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(GrammarProduction a, GrammarProduction b)
        {
            if (object.ReferenceEquals(a, b))
                return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return true;
            return !a.Equals(b);
        }
    }
}
