using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Common
{
    public class LineColumnMapping
    {
        public List<Location> _Locations = new List<Location>();

        public LineColumnMapping(string source)
        {
            SetLocations(source);
        }

        private void SetLocations(string source)
        {
            int line = 0, column = 0;

            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == '\n')
                {
                    _Locations.Add(new Location(line, column));

                    line++;
                    column = 0;
                }
                else
                {
                    _Locations.Add(new Location(line, column));

                    column++;
                }
            }
        }

        public Location GetLocation(int offset)
        {
            if (offset >= _Locations.Count)
            {
                var lastLocation = _Locations[_Locations.Count - 1];
                return new Location(lastLocation.Line, lastLocation.Column + 1);
            }

            return _Locations[offset];
        }
    }
}
