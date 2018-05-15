using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Compilador.Common;

namespace Compilador.Lalr
{
    public class SymbolString : ReadOnlyCollection<Symbol>, IEquatable<SymbolString>, IComparable<SymbolString>, IComparable
    {
        public SymbolString(IEnumerable<Symbol> symbols) : base(symbols.ToList())
        {
        }

        public int CompareTo(SymbolString other)
        {
            if (other == null)
                return 1;

            if (other.Count < Count)
                return 1;
            if (other.Count > Count)
                return -1;

            for (int i = 0; i < this.Count; i++)
            {
                var cp = this[i].CompareTo(other[i]);
                if (cp != 0)
                    return cp;
            }

            return 0;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as SymbolString);
        }

        public bool Equals(SymbolString other)
        {
            if (other == null)
                return false;

            if (this.Count != other.Count)
                return false;

            for (int i = 0; i < other.Count; i++)
            {
                if (this[i] != other[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SymbolString);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for (int i = 0; i < this.Count; i++)
            {
                hash = 31 * hash + this[i].GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            return string.Join(" ", this);
        }

        public static bool operator==(SymbolString a, SymbolString b){
            if (object.ReferenceEquals(a, b))
                return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator!=(SymbolString a, SymbolString b)
        {
            if (object.ReferenceEquals(a, b))
                return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return true;
            return !a.Equals(b);
        }

        private static readonly Memoize.FunctionName FirstSetName = Memoize.Function(nameof(FirstSet));
        public HashSet<TerminalSymbol> FirstSet(GrammarProductionDatabase db){
            return Memoize.Function(FirstSetName, Tuple.Create(this, db), (input) => input.Item1.InternalFirstSet(input.Item2));
        }

        private HashSet<TerminalSymbol> InternalFirstSet(GrammarProductionDatabase db){
            if (Count == 0)
                return new HashSet<TerminalSymbol>();

            HashSet<TerminalSymbol> ts = new HashSet<TerminalSymbol>();
            HashSet<NonterminalSymbol> analyzed = new HashSet<NonterminalSymbol>();

            InternalFirstSet(ts, analyzed, db);

            return ts;
        }

        public static SymbolString Concat(SymbolString symbolString, TerminalSymbol symbol)
        {
            List<Symbol> symbols = new List<Symbol>(symbolString);
            if (symbol != null)
                symbols.Add(symbol);
            return new SymbolString(symbols);
        }

        public SymbolString Range(int start)
        {
            List<Symbol> symbols = new List<Symbol>();
            for (int i = start; i < Count; i++)
            {
                symbols.Add(this[i]);
            }
            return new SymbolString(symbols);
        }

        private void InternalFirstSet(HashSet<TerminalSymbol> ts, HashSet<NonterminalSymbol> analyzed, GrammarProductionDatabase db){
            foreach (var item in this)
            {
                if(item is TerminalSymbol){
                    ts.Add(item as TerminalSymbol);
                    break;
                }
                else {
                    if (analyzed.Contains(item as NonterminalSymbol))
                    {
                        if (!db.IsNullable(item as NonterminalSymbol))
                            break;
                        else
                            continue;
                    }

                    analyzed.Add(item as NonterminalSymbol);
                    var rules = db[item as NonterminalSymbol];
                    foreach (var prod in rules)
                    {
                        prod.Body.InternalFirstSet(ts, analyzed, db);
                    }
                    if (!db.IsNullable(item as NonterminalSymbol))
                        break;
                }
            }
        }
    }

    public static class SymbolStringExtensions {
        public static SymbolString ToSymbolString(this IEnumerable<Symbol> symbols) => new SymbolString(symbols);
    }
}
