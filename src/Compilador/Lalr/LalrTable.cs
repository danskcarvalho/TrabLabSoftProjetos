using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Compilador.Lalr
{
    public class LalrTable : ReadOnlyCollection<LalrState>
    {
        public LalrTable(IList<LalrState> list) : base(list)
        {
        }

        public bool HasConflicts => this.Any(x => x.HasConflicts);
        public IEnumerable<LalrConflict> Conflicts => this.SelectMany(x => x.Conflicts);
    }
}
