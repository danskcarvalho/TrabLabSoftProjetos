using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilador.Lalr
{
    public class LalrItem : IEquatable<LalrItem>, IComparable<LalrItem>, IComparable
    {
        public GrammarProduction Production { get; private set; }
        public int ParsingPoint { get; private set; }
        public TerminalSymbol Lookahead { get; private set; }
        public LalrItem(GrammarProduction production, int parsingPoint, TerminalSymbol lookahead)
        {
            if (this.ParsingPoint > production.Body.Count || ParsingPoint < 0)
                throw new ArgumentOutOfRangeException(nameof(parsingPoint));
            
            this.Production = production;
            this.ParsingPoint = parsingPoint;
            this.Lookahead = lookahead;
        }

        public bool IsStartingProduction => ParsingPoint == 0 && Lookahead == TerminalSymbol.Eof && Production.Head == NonterminalSymbol.StartingSymbol;
        public bool IsKernel => ParsingPoint != 0 || IsStartingProduction;
        public Symbol CurrentSymbol => ParsingPoint < Production.Body.Count ? Production.Body[ParsingPoint] : null;
        public bool AtEnd => CurrentSymbol == null;

        public bool Equals(LalrItem other)
        {
            if (other == null)
                return false;

            return Production == other.Production && ParsingPoint == other.ParsingPoint && Lookahead == other.Lookahead;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LalrItem);
        }

        public override int GetHashCode() => 31 * (31 * Production.GetHashCode() + ParsingPoint.GetHashCode()) + (Lookahead?.GetHashCode() ?? 0);
    
        public override string ToString()
        {
            return $"[{GetProductionString()}, {(Lookahead?.ToString() ?? "\u2205")}]";
        }

        private string GetProductionString(){
            return $"{Production.Head.ToString()} = {string.Join(" ", Production.Body.Select((s, i) => (i == ParsingPoint ? "\u2022 " : "") + s.ToString()))}{ (ParsingPoint == Production.Body.Count ? " \u2022" : "") }";
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as LalrItem);
        }

        public int CompareTo(LalrItem other)
        {
            if (other == null)
                return 1;

            var cp = Production.CompareTo(other.Production);
            if (cp != 0)
                return cp;
            cp = ParsingPoint - other.ParsingPoint;
            if (cp != 0)
                return cp;
            if (Lookahead == null && other.Lookahead == null)
                return 0;
            else if (Lookahead == null)
                return -1;
            else
                return Lookahead.CompareTo(other.Lookahead);
        }

        public static bool operator ==(LalrItem a, LalrItem b)
        {
            if (object.ReferenceEquals(a, b))
                return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(LalrItem a, LalrItem b)
        {
            if (object.ReferenceEquals(a, b))
                return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return true;
            return !a.Equals(b);
        }

        public HashSet<TerminalSymbol> GetNextLookaheads(GrammarProductionDatabase db) {
            if (ParsingPoint == (Production.Body.Count - 1))
                return new HashSet<TerminalSymbol> { Lookahead };

            return SymbolString.Concat(Production.Body.Range(ParsingPoint + 1), Lookahead).FirstSet(db);
        }
    }
}
