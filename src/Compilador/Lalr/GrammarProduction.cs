using System;
using System.Collections.Generic;

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
            var cp = this.Head.CompareTo(other.Head);
            if (cp != 0)
                return cp;
            return this.Body.CompareTo(other.Body);
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

        public static GrammarProduction Create(string head, string bnf)
        {
            var tokenized = bnf.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<Symbol> symbols = new List<Symbol>();

            foreach (var tk in tokenized)
            {
                symbols.Add(CreateSymbol(tk));
            }
            return new GrammarProduction(new NonterminalSymbol(head), new SymbolString(symbols));
        }

        private static Symbol CreateSymbol(string tk)
        {
            if (tk.StartsWith("<"))
                return new NonterminalSymbol(tk.Substring(1, tk.Length - 2));
            else
                return new TerminalSymbol(tk);
        }
    }
}
