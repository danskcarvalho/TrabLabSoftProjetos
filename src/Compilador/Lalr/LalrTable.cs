using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Compilador.Lalr
{
    [Serializable]
    public class LalrTable : ReadOnlyCollection<LalrState>
    {
        public LalrTable(IList<LalrState> list) : base(list)
        {
        }
        private LalrTable() : base(new List<LalrState>())
        {

        }

        public bool HasConflicts => this.Any(x => x.HasConflicts);
        public IEnumerable<LalrConflict> Conflicts => this.SelectMany(x => x.Conflicts);
    }
}
