using System;
using System.Linq;
namespace Compilador.Lalr
{
    public enum LalrConflictType {
        ShiftReduce,
        ReduceReduce
    }
    public class LalrConflict
    {
        public LalrConflictType Type { get; private set; }
        public Symbol Symbol { get; private set; }
        public LalrState State { get; private set; }

        public LalrConflict(LalrConflictType type, Symbol symbol, LalrState state)
        {
            this.Type = type;
            this.Symbol = symbol;
            this.State = state;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case LalrConflictType.ReduceReduce:
                    return $"Conflito Reduce/Reduce para o símbolo {Symbol} e regras ({string.Join(", ", State[Symbol].Where(x => x is LalrReduce).Cast<LalrReduce>().Select(x => x.Production.ToString()))})";
                case LalrConflictType.ShiftReduce:
                    return $"Conflito Shift/Reduce para o símbolo {Symbol} e regras ({string.Join(", ", State[Symbol].Where(x => x is LalrReduce).Cast<LalrReduce>().Select(x => x.Production.ToString()))})";
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
