using Compilador.Common;
using Compilador.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Driving
{
    public static class Driver
    {
        public static void Run(string[] args)
        {
            var driver_args = GetArgs(args);
            var source = System.IO.File.ReadAllText(driver_args.Input);

            try
            {
                var grammar = Parser.Parse(source);
            }
            catch(GrammarException e)
            {
                DisplayError(e.Message);
            }
            catch
            {
                DisplayError("erro desconhecido");
            }
#if DEBUG
            Console.ReadKey(true);
#endif
        }

        private static void DisplayMessage(string message)
        {
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }

        private static void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static DriverArguments GetArgs(string[] args)
        {
            return new DriverArguments()
            {
                Input = args[0],
                Output = null //TODO: Mudar depois
            };
        }

        private class DriverArguments
        {
            public string Input { get; set; }
            public string Output { get; set; }
        }
    }
}
