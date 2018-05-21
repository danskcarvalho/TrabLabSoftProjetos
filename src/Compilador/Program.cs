using Compilador.Common;
using Compilador.Driving;
using Compilador.Lalr;
using Compilador.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Compilador.Common.Memoize;

namespace Compilador
{
    class Program
    {
        static void Main(string[] args)
        {
            Driver.Run(args);
        }
    }
}
