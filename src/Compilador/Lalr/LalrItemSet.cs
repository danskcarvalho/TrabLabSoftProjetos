using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Compilador.Common;

namespace Compilador.Lalr
{
    class LalrItemSet : ReadOnlyCollection<LalrItem>, IEquatable<LalrItemSet>
    {
        public LalrItemSet(IEnumerable<LalrItem> items) : base(new HashSet<LalrItem>(items).OrderBy(i => i).ToList())
        {
        }

        public bool Equals(LalrItemSet other)
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
            return Equals(obj as LalrItemSet);
        }

        private int? hashcode = null;
        public override int GetHashCode()
        {
            if (hashcode != null)
                return hashcode.Value;
            
            int hash = 0;
            for (int i = 0; i < this.Count; i++)
            {
                hash = 31 * hash + this[i].GetHashCode();
            }
            hashcode = hash;
            return hashcode.Value;
        }

        public override string ToString()
        {
            return "{" + string.Join(", ", this.Select(i => i.ToString())) + "}";
        }

        private static readonly Memoize.FunctionName ClosureName = Memoize.Function(nameof(Closure));
        public LalrItemSet Closure(GrammarProductionDatabase db){
            return Memoize.Function(ClosureName, Tuple.Create(this, db), (arg) => arg.Item1.InternalClosure(arg.Item2));
        }

        private LalrItemSet InternalClosure(GrammarProductionDatabase db){
            HashSet<LalrItem> added = new HashSet<LalrItem>();
            HashSet<LalrItem> toAdd = new HashSet<LalrItem>();
            HashSet<LalrItem> next = new HashSet<LalrItem>();

            toAdd.UnionWith(this);

            while (toAdd.Count != 0)
            {
                next.Clear();
                foreach (var item in toAdd)
                {
                    if (item.AtEnd || !(item.CurrentSymbol is NonterminalSymbol))
                        continue;

                    var productions = db[item.CurrentSymbol as NonterminalSymbol];
                    var nextLookaheads = item.GetNextLookaheads(db);
                    foreach (var p in productions)
                    {
                        foreach (var la in nextLookaheads)
                        {
                            var newItem = new LalrItem(p, 0, la);
                            if (!added.Contains(newItem) && !toAdd.Contains(newItem))
                                next.Add(newItem);
                        }
                    }
                }

                added.UnionWith(toAdd);
                toAdd.Clear();
                toAdd.UnionWith(next);
            }

            added.UnionWith(toAdd);
            added.UnionWith(next);
            return new LalrItemSet(added);
        }

        private static readonly Memoize.FunctionName GotoName1 = Memoize.Function(nameof(Goto));
        public LalrItemSet Goto(Symbol symbol){
            return Memoize.Function(GotoName1, Tuple.Create(this, symbol), (arg) => arg.Item1.InternalGoto(arg.Item2));
        }
        private LalrItemSet InternalGoto(Symbol symbol){
            HashSet<LalrItem> items = new HashSet<LalrItem>();
            foreach (var item in this)
            {
                if (item.AtEnd)
                    continue;

                if (item.CurrentSymbol == symbol)
                    items.Add(new LalrItem(item.Production, item.ParsingPoint + 1, item.Lookahead));
            }
            return new LalrItemSet(items);
        }

        private static readonly Memoize.FunctionName GotoName2 = Memoize.Function(nameof(Goto));
        public Dictionary<Symbol, LalrItemSet> Goto(GrammarProductionDatabase db)
        {
            return Memoize.Function(GotoName2, Tuple.Create(this, db), (arg) => arg.Item1.InternalGoto(arg.Item2));
        }
        private Dictionary<Symbol, LalrItemSet> InternalGoto(GrammarProductionDatabase db){
            HashSet<Symbol> symbols = new HashSet<Symbol>();

            var closure = this.Closure(db);
            foreach (var item in closure)
            {
                if (item.AtEnd)
                    continue;

                symbols.Add(item.CurrentSymbol);
            }

            Dictionary<Symbol, LalrItemSet> itemSets = new Dictionary<Symbol, LalrItemSet>();
            foreach (var symb in symbols)
            {
                itemSets[symb] = closure.Goto(symb);
            }
            return itemSets;
        }

        private static readonly Memoize.FunctionName ToKernelSetName = Memoize.Function(nameof(ToKernelSet));
        public LalrItemSet ToKernelSet()
        {
            return Memoize.Function(ToKernelSetName, this, (arg) => arg.InternalToKernelSet());
        }
        private LalrItemSet InternalToKernelSet() {
            HashSet<LalrItem> newItems = new HashSet<LalrItem>();
            foreach (var item in this)
            {
                if (item.IsKernel)
                    newItems.Add(item);
            }
            return new LalrItemSet(newItems);
        }

        private static readonly Memoize.FunctionName ToCoreSetName = Memoize.Function(nameof(ToCoreSet));
        public LalrItemSet ToCoreSet()
        {
            return Memoize.Function(ToCoreSetName, this, (arg) => arg.InternalToCoreSet());
        }
        private LalrItemSet InternalToCoreSet(){
            HashSet<LalrItem> newItems = new HashSet<LalrItem>();
            foreach (var item in this)
            {
                if (item.IsKernel)
                    newItems.Add(new LalrItem(item.Production, item.ParsingPoint, null));
            }
            return new LalrItemSet(newItems);
        }

        public static LalrItemSet Merge(LalrItemSet a, LalrItemSet b){
            HashSet<LalrItem> items = new HashSet<LalrItem>();
            foreach (var item in a)
            {
                items.Add(item);
            }
            foreach (var item in b)
            {
                items.Add(item);
            }
            return new LalrItemSet(items);
        }
    }
}
