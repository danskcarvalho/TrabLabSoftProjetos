using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in this)
            {
                foreach (var action in item.Value)
                {
                    builder.AppendLine($"{item.Key} {action}");
                }
            }
            return builder.ToString();
        }
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
            return $"shift {StateIndex}";
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
            return $"reduce {Production.ToString()}";
        }
    }
    [Serializable]
    public class LalrAccept : LalrAction {
        public LalrAccept() { }

        public override string ToString()
        {
            return "accept";
        }
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
            return $"goto {StateIndex}";
        }
    }
}
