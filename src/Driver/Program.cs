using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            var grammar = Compilador.Driving.Driver.Run(args);
            var parsed = API.Parsing.LalrParser.Parse(grammar, "(1 + 1)");
        }
    }
}
