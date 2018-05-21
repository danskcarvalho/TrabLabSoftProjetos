using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Compilador.Lalr
{
    [Serializable]
    public class LalrState : ReadOnlyDictionary<Symbol, ReadOnlyCollection<LalrAction>>
    {
        public LalrState(IDictionary<Symbol, ReadOnlyCollection<LalrAction>> dictionary) : base(dictionary)
        {
        }
        public LalrState() : base(new Dictionary<Symbol, ReadOnlyCollection<LalrAction>>())
        {

        }

        public bool HasConflicts => this.Any(x => x.Value.Count > 1);
        public IEnumerable<LalrConflict> Conflicts => this.Where(y => y.Value.Count > 1).Select(y => y.Value.Any(x => x is LalrShift) ?
                                                                                                new LalrConflict(LalrConflictType.ShiftReduce, y.Key, this) :
                                                                                                new LalrConflict(LalrConflictType.ReduceReduce, y.Key, this));
    }
    [Serializable]
    public abstract class LalrAction {
        protected LalrAction() { }
    }

    [Serializable]
    public class LalrShift : LalrAction {
        private int StateIndex { get; set; }
        private LalrTable Table { get; set; }

        public LalrShift(int stateIndex, LalrTable table){
            this.StateIndex = stateIndex;
            this.Table = table;
        }
        private LalrShift()
        {

        }

        public LalrState State => Table[StateIndex];

        public override string ToString()
        {
            return $"Shift {StateIndex}";
        }
    }
    [Serializable]
    public class LalrReduce : LalrAction {
        public GrammarProduction Production { get; private set; }

        public LalrReduce(GrammarProduction production){
            this.Production = production;
        }
        private LalrReduce()
        {

        }

        public override string ToString()
        {
            return $"Reduce {Production.ToString()}";
        }
    }
    [Serializable]
    public class LalrAccept : LalrAction {
        public LalrAccept() { }
    }
    [Serializable]
    public class LalrGoto : LalrAction {
        private int StateIndex { get; set; }
        private LalrTable Table { get; set; }

        private LalrGoto()
        {

        }
        public LalrGoto(int stateIndex, LalrTable table)
        {
            this.StateIndex = stateIndex;
            this.Table = table;
        }

        public LalrState State => Table[StateIndex];

        public override string ToString()
        {
            return $"Goto {StateIndex}";
        }
    }
}
