using System;
namespace Compilador.Lalr
{
    public abstract class Symbol : IComparable<Symbol>
    {
        public static bool operator==(Symbol a, Symbol b){
            if (object.ReferenceEquals(a, b))
                return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }
        public static bool operator!=(Symbol a, Symbol b){
            if (object.ReferenceEquals(a, b))
                return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return true;
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return object.ReferenceEquals(obj, this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(Symbol other)
        {
            if (other == null) //other comes before
                return 1;
            if (GetType() != other.GetType())
                return string.Compare(this.GetType().FullName, other.GetType().FullName, StringComparison.Ordinal);

            return (this as IComparable).CompareTo(other);
        }

        public abstract int CompareTo(object obj);
    }
}
