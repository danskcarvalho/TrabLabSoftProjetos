using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilador.Lalr
{
    [Serializable]
    public class GrammarProductionDatabase : IEquatable<GrammarProductionDatabase>
    {
        public IReadOnlyList<NonterminalSymbol> Nonterminals { get; private set; }
        public IReadOnlyList<GrammarProduction> Productions { get; private set; }
        public NonterminalSymbol StartingSymbol { get; private set; }

        protected GrammarProductionDatabase() { }
        public GrammarProductionDatabase(IEnumerable<GrammarProduction> productions,
                                         NonterminalSymbol startingSymbol)
        {
            var productionsPlusStart = productions.Union(
                new GrammarProduction[] { 
                new GrammarProduction(NonterminalSymbol.StartingSymbol, 
                                      new SymbolString(new Symbol[] {
                    startingSymbol
                })) 
            });
            this.Productions = new HashSet<GrammarProduction>(productionsPlusStart).OrderBy(p => p).ToList().AsReadOnly();
            this.Nonterminals = new HashSet<NonterminalSymbol>(this.Productions.Select(p => p.Head)).OrderBy(nt => nt).ToList().AsReadOnly();
            this.StartingSymbol = NonterminalSymbol.StartingSymbol;
            MapProductionsToNTSymbols();
            ComputeNullableSymbols();
        }

        private void MapProductionsToNTSymbols()
        {
            Dictionary<NonterminalSymbol, List<GrammarProduction>> map = new Dictionary<NonterminalSymbol, List<GrammarProduction>>();

            foreach (var item in Productions)
            {
                if (!map.ContainsKey(item.Head))
                    map[item.Head] = new List<GrammarProduction>();

                map[item.Head].Add(item);
            }

            mProductionsByNTSymbol = new Dictionary<NonterminalSymbol, IReadOnlyList<GrammarProduction>>();
            foreach (var item in map)
            {
                mProductionsByNTSymbol[item.Key] = item.Value.AsReadOnly();
            }
        }

        private Dictionary<NonterminalSymbol, IReadOnlyList<GrammarProduction>> mProductionsByNTSymbol;
        public IReadOnlyList<GrammarProduction> this[NonterminalSymbol nt] {
            get {
                return mProductionsByNTSymbol[nt];
            }
        }

        public bool IsNullable(NonterminalSymbol nt) {
            return mNullableSymbols.Contains(nt);
        }

        private HashSet<NonterminalSymbol> mNullableSymbols;
        private void ComputeNullableSymbols(){
            mNullableSymbols = new HashSet<NonterminalSymbol>();
            var nullCount = mNullableSymbols.Count;

            do
            {
                nullCount = mNullableSymbols.Count;

                foreach (var prod in Productions)
                {
                    if (TestNullable(prod))
                        mNullableSymbols.Add(prod.Head);
                }

            } while (nullCount != mNullableSymbols.Count());
        }

        private bool TestNullable(GrammarProduction prod)
        {
            if (prod.Body.Count == 0)
                return true;

            if (prod.Body.Any(s => s is TerminalSymbol))
                return false;

            return prod.Body.All(s => mNullableSymbols.Contains(s as NonterminalSymbol));
        }

        public bool Equals(GrammarProductionDatabase other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            if (this.Productions.Count != other.Productions.Count)
                return false;

            for (int i = 0; i < this.Productions.Count; i++)
            {
                if (this.Productions[i] != other.Productions[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GrammarProductionDatabase);
        }

        private int? hashcode = null;
        public override int GetHashCode()
        {
            if (hashcode != null)
                return hashcode.Value;

            int hash = 0;
            for (int i = 0; i < this.Productions.Count; i++)
            {
                hash = hash * 31 + this.Productions[i].GetHashCode();
            }
            hashcode = hash;

            return hashcode.Value;
        }

        public static bool operator ==(GrammarProductionDatabase a, GrammarProductionDatabase b)
        {
            if (object.ReferenceEquals(a, b))
                return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(GrammarProductionDatabase a, GrammarProductionDatabase b)
        {
            if (object.ReferenceEquals(a, b))
                return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return true;
            return !a.Equals(b);
        }
    }
}
